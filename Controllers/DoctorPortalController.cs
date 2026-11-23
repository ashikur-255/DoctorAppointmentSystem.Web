using DoctorAppointmentSystem.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorPortalController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ================= STEP 1: ENTER REGISTRATION NO =================
        public ActionResult SelectDoctor()
        {
            // If already selected, go to dashboard
            if (Session["DoctorId"] != null)
                return RedirectToAction("Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectDoctor(string regNo)
        {
            if (string.IsNullOrEmpty(regNo))
            {
                ViewBag.Error = "Registration Number is required!";
                return View();
            }

            var doctor = db.Doctors.FirstOrDefault(d => d.RegistrationNo == regNo);

            if (doctor == null)
            {
                ViewBag.Error = "Invalid Registration Number!";
                return View();
            }

            // 🔥 OPTIONAL: Link doctor to the logged-in user if not already linked
            var currentUserId = User.Identity.GetUserId();
            if (doctor.UserId == null)
            {
                doctor.UserId = currentUserId;
                db.SaveChanges();
            }

            // 🔥 Set session
            Session["DoctorId"] = doctor.Id;
            Session["DoctorName"] = doctor.Name;

            return RedirectToAction("Dashboard");
        }

        // ================= DASHBOARD =================
        public ActionResult Dashboard()
        {
            if (Session["DoctorId"] == null)
                return RedirectToAction("SelectDoctor");

            int doctorId = Convert.ToInt32(Session["DoctorId"]);
            DateTime today = DateTime.Today;

            var appointments = db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId &&
                            DbFunctions.TruncateTime(a.AppointmentDate) == today &&
                            a.IsConfirmed && !a.IsVisited)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            ViewBag.TotalPatients = appointments.Count;
            ViewBag.DoctorName = Session["DoctorName"];

            return View(appointments);
        }

        // ================= MARK VISITED =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MarkVisited(int id)
        {
            var app = db.Appointments.Find(id);
            if (app == null)
                return Json(new { success = false, message = "Appointment not found!" });

            app.IsVisited = true;
            db.SaveChanges();
            return Json(new { success = true });
        }

        // ================= LOGOUT =================
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            Request.GetOwinContext().Authentication.SignOut();
            // 🔥 Redirect to SelectDoctor page
    return RedirectToAction("SelectDoctor", "DoctorPortal");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}