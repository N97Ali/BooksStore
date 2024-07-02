using Books.DataAccess.Repository.IRepository;
using Books.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Books.DataAccess.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Books.DataAccess.Repository
{
    
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db):base(db)    
        {
                this._db = db;  
        }
       

        public void Update(OrderDetail orderDetail)
        {
             _db.OrderDetails.Update(orderDetail);
        }
    }


}
