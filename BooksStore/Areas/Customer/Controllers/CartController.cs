using Books.DataAccess.Repository.IRepository;
using Books.Models;
using Books.Models.ViewModels;
using BooksStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using SQLitePCL;
using Stripe.Checkout;
using System.Security.Claims;

namespace BooksStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _iunitOfWork;
    // [BindProperty]
        private ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork iunitOfWork)
        {

            _iunitOfWork = iunitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _iunitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _iunitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _iunitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;



            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        //[ActionName("Summary")]
        public IActionResult Summary(ShoppingCartVM ShoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM.ShoppingCartList = _iunitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


            ApplicationUser applicationUser = _iunitOfWork.ApplicationUser.Get(u => u.Id == userId);
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //its a ragular customer account needed to capture payment
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            }
            else
            {
                // compant user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;

            }
            _iunitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _iunitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    price = cart.Price,
                    count = cart.Count,

                };
                _iunitOfWork.OrderDetail.Add(orderDetail);
                _iunitOfWork.Save();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it a regular customer account and we need to capture payment
                // Stripe logic
                var domain = "https://localhost:44379/";
                var options = new  SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index", 
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price *100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new  SessionService();
               Session session =  service.Create(options);
                _iunitOfWork.OrderHeader.UpdateStripePaymentID (ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _iunitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);



            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _iunitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus !=SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower()=="paid")
                {
                    _iunitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _iunitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _iunitOfWork.Save();
                }
                //clear the cart before order placed
                HttpContext.Session.Clear();


            }
            List<ShoppingCart> shoppingCarts = _iunitOfWork.ShoppingCart.
                GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _iunitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _iunitOfWork.Save();
            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _iunitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _iunitOfWork.ShoppingCart.Update(cartFromDb);
            _iunitOfWork.Save();

            return RedirectToAction(nameof(Index));

        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _iunitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                //Remove that from cart
                _iunitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _iunitOfWork.ShoppingCart.GetAll(
              u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                cartFromDb.Count -= 1;
                _iunitOfWork.ShoppingCart.Update(cartFromDb);
            }


            _iunitOfWork.Save();

            return RedirectToAction(nameof(Index));

        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _iunitOfWork.ShoppingCart.Get(u => u.Id == cartId);

            _iunitOfWork.ShoppingCart.Remove(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart,_iunitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count()-1);
            _iunitOfWork.Save();

            return RedirectToAction(nameof(Index));

        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;

            }
            else if (shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;

            }
            else
            {
                return shoppingCart.Product.Price100;

            }
        }
    }
}
