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
    
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository 
    {
        private ApplicationDbContext _db;
        public ProductImageRepository(ApplicationDbContext db):base(db)    
        {
                this._db = db;  
        }
       

        public void Update(ProductImage productImage)
        {
             _db.ProductImages.Update(productImage);
        }
    }


}
