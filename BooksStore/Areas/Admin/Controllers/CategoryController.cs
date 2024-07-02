using Books.DataAccess.Data;
using Books.DataAccess.Repository.IRepository;
using Books.Models;
using BooksStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles=SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.Category.GetAll().ToList();
            return View(objCategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            //if (category.Name !=null )
            //{
            //    ModelState.AddModelError("","Please Enter the Detail");
            //}
            //if(category.Name ==category.DisplayOrder.ToString())
            //{
            //    ModelState.AddModelError("Name", "Name and displayOrder are not same ");
            //}
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["Success"] = "Category Created Successfully";
                return RedirectToAction("Index", "Category");
            }
            return View();

        }
        //----Edit data ----
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFind = _unitOfWork.Category.Get(u => u.Id == id);
            //Category? categoryFind1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //Category? categoryFind2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();
            if (categoryFind == null)
            {
                return NotFound();
            }
            return View(categoryFind);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["Success"] = "Category Edit Successfully";
                return RedirectToAction("Index", "Category");

            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFind = _unitOfWork.Category.Get(u => u.Id == id);
            //Category? categoryFind1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
            //Category? categoryFind2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();
            if (categoryFind == null)
            {
                return NotFound();
            }
            return View(categoryFind);
        }
        [HttpPost]
        public IActionResult Delete(Category category)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Remove(category);
                _unitOfWork.Save();
                TempData["Success"] = "Category Delete Successfully";
                return RedirectToAction("Index", "Category");

            }
            return View();
        }
    }
}
