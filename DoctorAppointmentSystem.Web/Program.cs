using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Configure global culture to use Bangladeshi Taka (৳) currency symbol
var cultureInfo = new System.Globalization.CultureInfo("en-US");
cultureInfo.NumberFormat.CurrencySymbol = "৳ ";
cultureInfo.NumberFormat.CurrencyPositivePattern = 0; // ৳ 1,000.00
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrate database & Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "Doctor", "User" };
        foreach (var roleName in roleNames)
        {
            if (!roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
            }
        }

        // Seed Admin user
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "admin@gmail.com";
        var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumber = "01700000000"
            };
            var createResult = userManager.CreateAsync(adminUser, "Admin@123").GetAwaiter().GetResult();
            if (createResult.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
            }
        }
        else
        {
            if (!userManager.IsInRoleAsync(adminUser, "Admin").GetAwaiter().GetResult())
            {
                userManager.AddToRoleAsync(adminUser, "Admin").GetAwaiter().GetResult();
            }
            if (!userManager.CheckPasswordAsync(adminUser, "Admin@123").GetAwaiter().GetResult())
            {
                var token = userManager.GeneratePasswordResetTokenAsync(adminUser).GetAwaiter().GetResult();
                userManager.ResetPasswordAsync(adminUser, token, "Admin@123").GetAwaiter().GetResult();
            }
        }

        // Seed Doctor user (doctor@gmail.com / Doctor@123)
        var doctorEmail = "doctor@gmail.com";
        var doctorUser = userManager.FindByEmailAsync(doctorEmail).GetAwaiter().GetResult();
        if (doctorUser == null)
        {
            doctorUser = new ApplicationUser
            {
                UserName = doctorEmail,
                Email = doctorEmail,
                EmailConfirmed = true,
                PhoneNumber = "01722222222"
            };
            var createDoctorResult = userManager.CreateAsync(doctorUser, "Doctor@123").GetAwaiter().GetResult();
            if (createDoctorResult.Succeeded)
            {
                userManager.AddToRoleAsync(doctorUser, "Doctor").GetAwaiter().GetResult();
            }
        }
        else
        {
            if (!userManager.IsInRoleAsync(doctorUser, "Doctor").GetAwaiter().GetResult())
            {
                userManager.AddToRoleAsync(doctorUser, "Doctor").GetAwaiter().GetResult();
            }
            if (!userManager.CheckPasswordAsync(doctorUser, "Doctor@123").GetAwaiter().GetResult())
            {
                var token = userManager.GeneratePasswordResetTokenAsync(doctorUser).GetAwaiter().GetResult();
                userManager.ResetPasswordAsync(doctorUser, token, "Doctor@123").GetAwaiter().GetResult();
            }
        }

        // Seed Normal User (user@gmail.com / User@123, Phone: 01711111111)
        var normalUserEmail = "user@gmail.com";
        var normalUser = userManager.FindByEmailAsync(normalUserEmail).GetAwaiter().GetResult();
        if (normalUser == null)
        {
            normalUser = new ApplicationUser
            {
                UserName = "01711111111",
                Email = normalUserEmail,
                EmailConfirmed = true,
                PhoneNumber = "01711111111"
            };
            var createUserResult = userManager.CreateAsync(normalUser, "User@123").GetAwaiter().GetResult();
            if (createUserResult.Succeeded)
            {
                userManager.AddToRoleAsync(normalUser, "User").GetAwaiter().GetResult();
            }
        }
        else
        {
            if (!userManager.IsInRoleAsync(normalUser, "User").GetAwaiter().GetResult())
            {
                userManager.AddToRoleAsync(normalUser, "User").GetAwaiter().GetResult();
            }
            if (!userManager.CheckPasswordAsync(normalUser, "User@123").GetAwaiter().GetResult())
            {
                var token = userManager.GeneratePasswordResetTokenAsync(normalUser).GetAwaiter().GetResult();
                userManager.ResetPasswordAsync(normalUser, token, "User@123").GetAwaiter().GetResult();
            }
        }

        // One-time fix to resolve existing duplicate doctor photos and ensure distinct images
        try
        {
            bool needsFix = false;
            var allDoctors = db.Doctors.ToList();

            foreach (var doc in allDoctors)
            {
                if (string.IsNullOrEmpty(doc.ImagePath)) continue;
                // NEVER touch any doctor photo uploaded by the admin (Base64 data URI)
                if (doc.ImagePath.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;

                // Fix Dr. Selina Sarkar: must have unique female doctor photo, not male doctor photo
                if (doc.Name.Contains("Selina", StringComparison.OrdinalIgnoreCase) &&
                    (doc.ImagePath.Contains("cf5e63ac") || !doc.ImagePath.Contains("745379ff")))
                {
                    doc.ImagePath = "/Images/Doctors/745379ff-dc59-45c1-a8cc-993f65eb672d.png";
                    needsFix = true;
                }

                // Fix Dr. Sabrina Mostafa: unique female doctor photo
                if (doc.Name.Contains("Sabrina Mostafa", StringComparison.OrdinalIgnoreCase) &&
                    (doc.ImagePath.Contains("be68a3a8") || !doc.ImagePath.Contains("0156bed4")))
                {
                    doc.ImagePath = "/Images/Doctors/0156bed4-4e2d-405a-9860-8bbcbeb2342b.png";
                    needsFix = true;
                }

                // Fix Dr. Md. Asif Masud: unique male doctor photo
                if (doc.Name.Contains("Asif Masud", StringComparison.OrdinalIgnoreCase) &&
                    doc.ImagePath.Contains("be68a3a8"))
                {
                    doc.ImagePath = "/Images/Doctors/aa8e8e97-dc6f-40d1-8aed-8da8ad56bf92.png";
                    needsFix = true;
                }

                // Fix duplicate Dr. Ayubur Rahman: unique male doctor photo
                if (doc.Name.Contains("Ayubur", StringComparison.OrdinalIgnoreCase) &&
                    doc.ImagePath.Contains("557a6790"))
                {
                    doc.ImagePath = "/Images/Doctors/4c28ae9d-15bd-4a90-a490-fddd3770b21e.png";
                    needsFix = true;
                }
            }

            if (needsFix)
            {
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(ex, "Could not apply one-time doctor image fix.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles or connecting to DB.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
