 
using Books.DataAccess.Data;
using Books.DataAccess.Repository.IRepository;
using Books.Models;
using Books.Models.ViewModels;
using BooksStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BooksStore.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class  UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole>  _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;

        }
        public IActionResult Index()
        {
            return View();
        } 
        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };
            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == userId))
                .GetAwaiter().GetResult().FirstOrDefault();       
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id))
                .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);


            if (!(roleManagementVM.ApplicationUser.Role== oldRole))
            {
                //role was update 
                if(roleManagementVM.ApplicationUser.Role ==SD.Role_Company) {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();
                _userManager.RemoveFromRoleAsync(applicationUser,oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser,roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole == SD.Role_Company && applicationUser.CompanyId != roleManagementVM.ApplicationUser.CompanyId) 
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }
           
            return RedirectToAction("Index");
        }



        #region API CALLS
        public IActionResult GetAll()
        {           
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties: "company").ToList();
           // var userRoles = _db.UserRoles.ToList();
           // var roles  = _db.Roles.ToList();    

            foreach (var user in objUserList)
            {
             //   var roleId = userRoles.FirstOrDefault(u=>u.UserId ==user.Id ).RoleId;
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if(user.company ==  null)
                {
                    user.company = new() { Name = "" };
                }
            }
            return Json(new { data = objUserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string  id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
            if(objFromDb == null) {

                return Json(new { success = false, message = "Error while locking/Unlocking " });
                
            }
            if(objFromDb.LockoutEnd !=null && objFromDb.LockoutEnd>DateTime.Now) {
            //user is currently locked and we need to unlock them 
            objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd= DateTime.Now.AddYears(1000);
            }
           _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Opration Successfully" });
       
        
        
        }
        #endregion  
    }
}
 