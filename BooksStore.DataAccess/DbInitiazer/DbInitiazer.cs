using Books.DataAccess.Data;
using Books.Models;
using BooksStore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Books.DataAccess.DbInitiazer
{
    public class DbInitiazer : IDbInitiazer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitiazer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _userManager = userManager;
            _db = db;


        }
        public void Initiazer()
        {
            //migration if thay are not applied 
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { }
            //create role if they are not created 
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
                // if roles are not created , then we will create admin user as well

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@Naushadali.com",
                    Email = "admin@Naushadali.com",
                    Name = "Naushad Ali",
                    PhoneNumber = "123456",
                    StreetAddress = "Test Mbd",
                    State = "IL",
                    PostalCode = "7541654",
                    City = "Delhi"
                }, "admin@Naushadali.com1").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@Naushadali.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
           
        }
    }
 }
 
