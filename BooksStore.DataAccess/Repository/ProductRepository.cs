using Books.DataAccess.Repository.IRepository;
using Books.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Books.DataAccess.Data;

namespace Books.DataAccess.Repository
{

    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            this._db = db;
        }


        public void Update(Product product)
        {
            var productFromdb = _db.Products.FirstOrDefault(u => u.Id == product.Id);
            if (productFromdb != null)
            {
                productFromdb.Title = product.Title;
                productFromdb.ISBN = product.ISBN;
                productFromdb.Price = product.Price;
                productFromdb.Price50 = product.Price50;
                productFromdb.Price100 = product.Price100;
                productFromdb.Description = product.Description;
                productFromdb.CategoryId = product.CategoryId;
                productFromdb.Author = product.Author;
                if (productFromdb.ImageUrl != null)
                {
                    productFromdb.ImageUrl = product.ImageUrl;
                }
            }
        }
    }


}
