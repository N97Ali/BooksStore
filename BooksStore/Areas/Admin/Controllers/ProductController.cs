using Books.DataAccess.Data;
using Books.DataAccess.Repository.IRepository;
using Books.Models;
using Books.Models.ViewModels;
using BooksStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

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
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id,includeProperties:"ProductImages");
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM,List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    //TempData["Success"] = "Product Create Successfully";

                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                  //  TempData["Success"] = "Product Update Successfully";
                }
                _unitOfWork.Save();
                string  wwwRootPath = _webHostEnvironment.WebRootPath;
                if(files != null)
                {

                    foreach (var file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"images\products\produt-"+productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        ProductImage productImage = new ProductImage()
                        {
                            ImageUrl = @"\"+productPath+@"\"+fileName,
                            ProductId = productVM.Product.Id,
                        };
                        if(productVM.Product.ProductImages== null)
                            productVM.Product.ProductImages= new List<ProductImage>();


                        productVM.Product.ProductImages.Add(productImage);
                      

                    }
                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();

                

                    //productVM.Product.ImageUrl = @"\Images\Product\" + fileName;
                }
                TempData["Success"] = "Product Update/Create Successfully";

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

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(u=>u.id == imageId); 
            int productId = imageToBeDeleted.ProductId; 
            if( imageToBeDeleted != null)
            {
                if(!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                        var oldImagePath = 
                        Path.Combine(_webHostEnvironment.WebRootPath ,
                        imageToBeDeleted.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();
               // TempData["success"] = "Delete Successfully";
            }
            return RedirectToAction(nameof(Upsert), new {id = productId });
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
            //var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.Trim('\\'));
            //if (System.IO.File.Exists(oldImagePath))
            //{
            //    System.IO.File.Delete(oldImagePath);
            //}
            string productPath = @"images\products\produt-" +id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach(string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }
                

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
        #endregion
    }
}
