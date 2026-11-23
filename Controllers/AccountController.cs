using DoctorAppointmentSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;

        public AccountController() { }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        private IAuthenticationManager AuthenticationManager =>
            HttpContext.GetOwinContext().Authentication;

        // ================= LOGIN (GET) =================
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // ================= LOGIN (POST) =================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            ApplicationUser user = null;

            // 🔹 Check if input looks like phone number (BD format)
            if (System.Text.RegularExpressions.Regex.IsMatch(model.LoginInput, @"^(01[3-9]\d{8})$"))
            {
                // 👉 USER LOGIN (Phone)
                user = await UserManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == model.LoginInput);
            }
            else
            {
                // 👉 ADMIN / DOCTOR LOGIN (Email)
                user = await UserManager.FindByEmailAsync(model.LoginInput);
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // 🔹 Login using username internally
            var result = await SignInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                shouldLockout: false);

            if (result == SignInStatus.Success)
            {
                var roles = await UserManager.GetRolesAsync(user.Id);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                if (roles.Contains("Doctor"))
                    return RedirectToAction("SelectDoctor", "DoctorPortal");

                // ✅ USER LOGIN (Phone based)
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        // ================= REGISTER (GET) =================
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // ================= REGISTER (POST) =================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ================= ONE-TIME DOCTOR CHECK =================
            if (model.Role == "Doctor")
            {
                var context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context));

                // Ensure Doctor role exists
                if (!await roleManager.RoleExistsAsync("Doctor"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Doctor"));
                }

                // Get RoleId for Doctor
                var doctorRoleId = roleManager.FindByName("Doctor").Id;

                // Check if any user already has Doctor role
                var usersInDoctorRole = await UserManager.Users
                    .Where(u => u.Roles.Any(r => r.RoleId == doctorRoleId))
                    .ToListAsync();

                if (usersInDoctorRole.Count > 0)
                {
                    ModelState.AddModelError("", "A Doctor account already exists. Cannot create another.");
                    return View(model);
                }
            }

            // ================= CREATE USER =================
            var user = new ApplicationUser
            {
                UserName = model.PhoneNumber,   // login with phone
                PhoneNumber = model.PhoneNumber,

                // 🔥 FIX: handle empty email
                Email = string.IsNullOrEmpty(model.Email)
             ? model.PhoneNumber + "@noemail.com"
             : model.Email,

                EmailConfirmed = true
            };

            var result = await UserManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // ================= ENSURE ROLE EXISTS =================
                var roleToAssign = string.IsNullOrEmpty(model.Role) ? "User" : model.Role;
                await EnsureRoleExists(roleToAssign);
                await UserManager.AddToRoleAsync(user.Id, roleToAssign);

                // ================= AUTO LOGIN =================
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                // ================= ROLE BASED REDIRECT =================
                if (roleToAssign == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                if (roleToAssign == "Doctor")
                    return RedirectToAction("SelectDoctor", "DoctorPortal");

                // Normal User
                return RedirectToAction("Index", "Home");
            }

            // ================= HANDLE ERRORS =================
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);

            return View(model);
        }

        // ================= FORGOT PASSWORD (GET) =================
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // ================= FORGOT PASSWORD (POST) =================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return View("ForgotPasswordConfirmation");
            }

            // Generate token
            var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            // Create URL
            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new
                {
                    code = code
                },
                protocol: Request.Url.Scheme);

            // Send Email
            await UserManager.SendEmailAsync(
                user.Id,
                "Reset Password",
                $"Please reset your password by clicking here: <br/><br/>" +
                $"<a href=\"{callbackUrl}\">Reset Password</a>");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // ================= RESET PASSWORD (GET) =================
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            if (code == null)
                return View("Error");

            var model = new ResetPasswordViewModel
            {
                Code = code
            };

            return View(model);
        }

        // ================= RESET PASSWORD (POST) =================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await UserManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await UserManager.ResetPasswordAsync(
                user.Id,
                model.Code,
                model.Password);

            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);

            return View(model);
        }

        // ================= FORGOT PASSWORD CONFIRM =================
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ================= RESET PASSWORD CONFIRM =================
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // ================= ENSURE ROLE HELPER =================
        private async Task EnsureRoleExists(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                roleName = "User";

            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(new ApplicationDbContext()));

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // ================= LOGOUT =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        
    }
}