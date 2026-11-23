using DoctorAppointmentSystem.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DoctorAppointmentSystem.Web.Controllers
{
    public class DoctorAccountController : BaseController
    {
        public DoctorAccountController(ApplicationDbContext db) : base(db)
        {
        }

        // Login Page
        public IActionResult Login()
        {
            return View();
        }

        // Login Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string registrationNo)
        {
            if (string.IsNullOrEmpty(registrationNo))
            {
                ViewBag.Error = "Please enter Registration Number";
                return View();
            }

            var doctor = _db.Doctors
                .FirstOrDefault(d => d.RegistrationNo == registrationNo);

            if (doctor != null)
            {
                HttpContext.Session.SetInt32("DoctorId", doctor.Id);
                HttpContext.Session.SetString("DoctorName", doctor.Name);

                return RedirectToAction("Dashboard", "DoctorPortal");
            }

            ViewBag.Error = "Invalid Registration Number";
            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
