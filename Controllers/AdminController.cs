using System;
using System.Linq;
using System.Web.Mvc;
using DoctorAppointmentSystem.Models;
using System.Data.Entity;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =========================
        // INDEX
        // =========================
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        // =========================
        // DASHBOARD
        // =========================
        public ActionResult Dashboard()
        {
            var model = new DashboardVM();

            // =========================
            // 📊 COUNTS
            // =========================
            model.TotalDoctors = db.Doctors.Count();
            model.TotalPatients = db.Patients.Count();
            model.TotalAppointments = db.Appointments.Count();
            model.TotalSpecializations = db.Specializations.Count();

            DateTime today = DateTime.Today;

            model.TodayAppointments = db.Appointments
                .Count(a => DbFunctions.TruncateTime(a.AppointmentDate) == today);

            // =========================
            // 📈 CHART DATA
            // =========================
            var data = db.Doctors
                .Include(d => d.Specialization)
                .Where(d => d.Specialization != null)
                .GroupBy(d => d.Specialization.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .ToList();

            model.SpecializationNames = data.Select(x => x.Name).ToList();
            model.DoctorCounts = data.Select(x => x.Count).ToList();

            // =========================
            // 👤 ADMIN NAME (FIXED)
            // =========================
            var loginInput = User.Identity.Name;

            // Try to get admin from Identity table
            var admin = db.Users.FirstOrDefault(u => u.UserName == loginInput);

            if (admin != null)
            {
                model.UserName = admin.UserName; // or FullName
            }
            else
            {
                model.UserName = loginInput;
            }

            // =========================
            // OPTIONAL: GLOBAL VIEWBAG (for Layout)
            // =========================
            ViewBag.UserName = model.UserName;

            return View(model);
        }

        // =========================
        // DISPOSE
        // =========================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}