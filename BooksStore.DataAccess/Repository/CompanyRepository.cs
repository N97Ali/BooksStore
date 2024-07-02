using Books.DataAccess.Data;
using Books.DataAccess.Repository.IRepository;
using Books.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Books.DataAccess.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {


        private readonly ApplicationDbContext _db;
        public CompanyRepository(ApplicationDbContext db) : base(db)
        {
            this._db = db;
        }

        public void Update(Company company)
        {
            var companyFromdb = _db.Companies.FirstOrDefault(u => u.Id == company.Id);
            if (companyFromdb != null)
            {
                companyFromdb.Name = company.Name;
                companyFromdb.StreetAddress = company.StreetAddress;
                companyFromdb.City = company.City;
                companyFromdb.State = company.State;
                companyFromdb.PostalCode = company.PostalCode;
                companyFromdb.PhoneNumber = company.PhoneNumber;
            }
        }
    }
    
}
