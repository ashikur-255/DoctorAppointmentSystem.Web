using DoctorAppointmentSystem.Models;
using System.Linq;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    public class HomeController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Home
        public ActionResult Index()
        {
            var doctors = db.Doctors
                            .Include("Specialization")
                            .ToList();

            return View(doctors);
        }

        // GET: Home/About
        public ActionResult About()
        {
            ViewBag.DoctorCount = db.Doctors.Count();
            ViewBag.PatientCount = db.Patients.Count();
            ViewBag.AppointmentCount = db.Appointments.Count();
            ViewBag.SpecializationCount = db.Specializations.Count();

            return View();
        }


    }
}