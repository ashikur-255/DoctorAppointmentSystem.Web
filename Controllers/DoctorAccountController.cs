using System.Linq;
using System.Web.Mvc;
using DoctorAppointmentSystem.Models;

namespace DoctorAppointmentSystem.Controllers
{
    public class DoctorAccountController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Login Page
        public ActionResult Login()
        {
            return View();
        }

        // Login Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string registrationNo)
        {
            if (string.IsNullOrEmpty(registrationNo))
            {
                ViewBag.Error = "Please enter Registration Number";
                return View();
            }

            var doctor = db.Doctors
                .FirstOrDefault(d => d.RegistrationNo == registrationNo);

            if (doctor != null)
            {
                Session["DoctorId"] = doctor.Id;
                Session["DoctorName"] = doctor.Name;

                return RedirectToAction("Dashboard", "DoctorPortal");
            }

            ViewBag.Error = "Invalid Registration Number";
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}