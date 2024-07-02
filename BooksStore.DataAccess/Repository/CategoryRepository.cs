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
    
    public class CategoryRepository : Repository<Category>, ICategoryRepository 
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db):base(db)    
        {
                this._db = db;  
        }
       

        public void Update(Category category)
        {
             _db.Categories.Update(category);
        }
    }


}
