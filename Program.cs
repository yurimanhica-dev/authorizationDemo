using AuthorizationDemo.Data;
using AuthorizationDemo.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            .EnableSensitiveDataLogging()
            .LogTo(Console.WriteLine));

// Identity com ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    var permissions = builder.Services.BuildServiceProvider().GetRequiredService<AppDbContext>()
    .Permissions.ToList();

    foreach (var perm in permissions)
    {
        options.AddPolicy(perm.Name, policy =>
            policy.RequireClaim("Permission", perm.Name));
    }
});

// Authorization
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

// Controllers + Views
builder.Services.AddControllersWithViews();

var app = builder.Build();

#region SeedData
// // Carregar policies dinamicamente
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     var permissions = db.Permissions.ToList();

//     var authOptions = scope.ServiceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

//     foreach (var p in permissions)
//     {
//         authOptions.AddPolicy(p.Name, policy =>
//             policy.RequireClaim("Permission", p.Name));
//     }
// }

#endregion

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
