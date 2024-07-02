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
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
          
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)//update and insert
        {

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if(id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string  wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"Images\Product"); 
                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath,productVM.Product.ImageUrl.Trim('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);    
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\Images\Product\" + fileName;
                }
                if(productVM.Product.Id== 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    TempData["Success"] = "Product Create Successfully";

                }
               else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    TempData["Success"] = "Product Update Successfully";
                }
                _unitOfWork.Save();
               
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }
        //----Edit data ----
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? productFind = _unitOfWork.Product.Get(u => u.Id == id);
        //    //Product? ProductFind1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
        //    //Product? ProductFind2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();
        //    if (productFind == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFind);
        //}
        //[HttpPost]
        //public IActionResult Edit(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(product);
        //        _unitOfWork.Save();
        //        TempData["Success"] = "Product Edit Successfully";
        //        return RedirectToAction("Index", "Product");

        //    }
        //    return View();
        ////}
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? productFind = _unitOfWork.Product.Get(u => u.Id == id);
        //    //Product? ProductFind1 = _db.Categories.FirstOrDefault(u=>u.Id==id);
        //    //Product? ProductFind2 = _db.Categories.Where(u=>u.Id==id).FirstOrDefault();
        //    if (productFind == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(productFind);
        //}
        //[HttpPost]
        //public IActionResult Delete(Product product)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Remove(product);
        //        _unitOfWork.Save();
        //        TempData["Success"] = "Product Delete Successfully";
        //        return RedirectToAction("Index", "Product");

        //    }
        //    return View();
        //}


        #region API CALLS
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data =  objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleteing " });
            }
            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
