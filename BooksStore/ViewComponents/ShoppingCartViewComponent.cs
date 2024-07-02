using Books.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using BooksStore.Utility;
using System.Security.Claims;

namespace BooksStore.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _iunitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork iunitOfWork)
        {
            _iunitOfWork = iunitOfWork;

        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                if(HttpContext.Session.GetInt32(SD.SessionCart)== null)
                {

                    HttpContext.Session.SetInt32(SD.SessionCart,
                        _iunitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId ==
                        claim.Value).Count());
                }


                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
