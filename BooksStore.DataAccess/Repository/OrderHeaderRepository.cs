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

    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            this._db = db;
        }


        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFormdb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderFormdb != null)
            {
                orderFormdb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFormdb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntendId)
        {
            var orderFormdb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFormdb.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntendId))
            {
                orderFormdb.PaymentIntendId = paymentIntendId;
                orderFormdb.PaymentDate = DateTime.Now;
            }
        }
    }


}
