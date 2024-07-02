using Books.DataAccess.Repository;
using Books.DataAccess.Repository.IRepository;
using Books.Models;
using Books.Models.ViewModels;
using BooksStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BooksStore.Areas.Admin.Controllers
{

    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _iunitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }    
        public OrderController(IUnitOfWork iunitOfWork)
        {
                _iunitOfWork = iunitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _iunitOfWork.OrderHeader.Get(u=>u.Id ==orderId , includeProperties:"ApplicationUser"),
                OrderDetail = _iunitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId == orderId , includeProperties : "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StrarProcessing()
        {
            _iunitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _iunitOfWork.Save();
            TempData["Success"] = "Order Detail Update successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancleOrder()
        {
            var orderHeader = _iunitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if(orderHeader.PaymentStatus== SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntendId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _iunitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled,SD.StatusRefunded);
            }
            else
            {
                _iunitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _iunitOfWork.Save();
            TempData["Success"] = "Order Cancle successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrder()
        {
            var orderHeader = _iunitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrakingNumber = OrderVM.OrderHeader.TrakingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate =DateTime.Now;
            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {

                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);

            }



            _iunitOfWork.OrderHeader.Update(orderHeader);
            _iunitOfWork.Save();
            TempData["Success"] = "Order Shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_Pay_Now()
        {
            OrderVM.OrderHeader= _iunitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.OrderHeader.Id, 
                includeProperties:"ApplicationUser");

            OrderVM.OrderDetail = _iunitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id,
                includeProperties: "Product");


            var domain = "https://localhost:44379/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in  OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.price*100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _iunitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _iunitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _iunitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _iunitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _iunitOfWork.OrderHeader.UpdateStatus(orderHeaderId,orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _iunitOfWork.Save();
                }
            }
    
            return View(orderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFormDb = _iunitOfWork.OrderHeader.Get(u=>u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFormDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFormDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFormDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFormDb.City = OrderVM.OrderHeader.City;
            orderHeaderFormDb.State = OrderVM.OrderHeader.State;
            orderHeaderFormDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                {
                orderHeaderFormDb.Carrier= OrderVM.OrderHeader.Carrier; 
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrakingNumber))
                {
                orderHeaderFormDb.TrakingNumber = OrderVM.OrderHeader.TrakingNumber;
            }
            _iunitOfWork.OrderHeader.Update(orderHeaderFormDb);
            _iunitOfWork.Save();
            TempData["Success"] = "Order Detail Update successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFormDb.Id });
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeader = _iunitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            if(User.IsInRole(SD.Role_Admin)|| User.IsInRole(SD.Role_Employee))
            {
                objOrderHeader = _iunitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeader = _iunitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId== userId, includeProperties: "ApplicationUser");


            }

            switch (status)
            {
                case "pending":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;

                case "completed":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;

                case "approved":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeader });
        }
        #endregion
    }
}
