using Books.DataAccess.Data;
using Books.DataAccess.Repository.IRepository;
using Books.Models;
using Books.Models.ViewModels;
using BooksStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BooksStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class  CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objcompanyList = _unitOfWork.Company.GetAll().ToList();

            return View(objcompanyList);
        }
        public IActionResult Upsert(int? id)//update and insert
        {
            
            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company company = _unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                
                if (company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                    TempData["Success"] = "company Create Successfully";

                }
                else
                {
                    _unitOfWork.Company.Update(company);
                    TempData["Success"] = "company Update Successfully";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }
   

        #region API CALLS
        public IActionResult GetAll()
        {
            List<Company> objcompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objcompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleteing " });
            }
            
            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion  
    }
}
