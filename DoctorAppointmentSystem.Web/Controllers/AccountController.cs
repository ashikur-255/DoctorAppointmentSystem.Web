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

            var input = model.LoginInput?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            ApplicationUser user = null;

            // 1. Find by Email
            user = await _userManager.FindByEmailAsync(input);

            // 2. Find by UserName
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(input);
            }

            // 3. Find by Phone Number
            if (user == null)
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == input);
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

            model.PhoneNumber = model.PhoneNumber?.Trim();
            model.Email = model.Email?.Trim();

            var roleToAssign = string.IsNullOrEmpty(model.Role) ? "User" : model.Role;

            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleToAssign));
            }

            // Check if phone number is already registered
            var existingByPhone = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber || u.UserName == model.PhoneNumber);
            if (existingByPhone != null)
            {
                ModelState.AddModelError("PhoneNumber", "This phone number is already registered. Please log in.");
                return View(model);
            }

            // Check if email is already registered
            if (!string.IsNullOrEmpty(model.Email))
            {
                var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingByEmail != null)
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }
            }

            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,
                PhoneNumber = model.PhoneNumber,
                Email = string.IsNullOrEmpty(model.Email)
                    ? model.PhoneNumber + "@clickebd.xyz"
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

        // FORGOT PASSWORD (GET)
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // FORGOT PASSWORD (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email?.Trim());
                if (user != null)
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { code = code }, protocol: HttpContext.Request.Scheme);
                    TempData["ResetLink"] = callbackUrl;
                }
                return View("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        // FORGOT PASSWORD CONFIRMATION
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // RESET PASSWORD (GET)
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View(new ResetPasswordViewModel { Code = code });
        }

        // RESET PASSWORD (POST)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email?.Trim());
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // RESET PASSWORD CONFIRMATION
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
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
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
