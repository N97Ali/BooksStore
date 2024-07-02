   using Books.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Books.DataAccess.Repository;
using Books.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Books.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Books.DataAccess.DbInitiazer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "834041208621914";
    options.AppSecret = "f025522ab1487e9407e4e36ec06d10df";

});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout= TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly =true;
    options.Cookie.IsEssential= true;

});


builder.Services.AddScoped<IDbInitiazer, DbInitiazer>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:StripeKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeeDataBase();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
void SeeDataBase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitiazer = scope.ServiceProvider.GetRequiredService<IDbInitiazer>();
    }
}