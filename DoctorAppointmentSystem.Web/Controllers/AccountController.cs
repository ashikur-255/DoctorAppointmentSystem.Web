using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoctorAppointmentSystem.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager) : base(db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // LOGIN (GET)
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // LOGIN (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            await _signInManager.SignOutAsync();

            ApplicationUser user = null;

            // Check if input looks like BD phone number
            if (Regex.IsMatch(model.LoginInput, @"^(01[3-9]\d{8})$"))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.LoginInput);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(model.LoginInput);
                }
            }
            else
            {
                user = await _userManager.FindByEmailAsync(model.LoginInput);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(model.LoginInput);
                }
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                if (roles.Contains("Doctor"))
                    return RedirectToAction("SelectDoctor", "DoctorPortal");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // REGISTER (GET)
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var roleToAssign = string.IsNullOrEmpty(model.Role) ? "User" : model.Role;

            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
            }

            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,
                PhoneNumber = model.PhoneNumber,
                Email = string.IsNullOrEmpty(model.Email)
                    ? model.PhoneNumber + "@noemail.com"
                    : model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleToAssign);
                await _signInManager.SignInAsync(user, isPersistent: false);

                if (roleToAssign == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                if (roleToAssign == "Doctor")
                    return RedirectToAction("SelectDoctor", "DoctorPortal");

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // LOGOFF (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ACCESS DENIED
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
