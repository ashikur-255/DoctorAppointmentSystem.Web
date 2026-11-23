using DoctorAppointmentSystem.Models;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize]
    public class AppointmentsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =========================
        // INDEX
        // =========================
        public ActionResult Index(string search, string status, string sortOrder, int? page)
        {
            // ✅ BLOCK NORMAL USER
            if (User.IsInRole("User"))
            {
                return RedirectToAction("MyAppointments");
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSort = sortOrder == "date_desc" ? "date_asc" : "date_desc";

            var appointments = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            // =========================
            // ✅ DOCTOR FILTER
            // =========================

            // Session doctor (Registration No)
            if (Session["DoctorId"] != null)
            {
                int doctorId = (int)Session["DoctorId"];
                appointments = appointments.Where(a => a.DoctorId == doctorId);
            }
            // Identity doctor
            else if (User.IsInRole("Doctor"))
            {
                var email = User.Identity.Name;
                var doctor = db.Doctors.FirstOrDefault(d => d.Email == email);

                if (doctor != null)
                {
                    appointments = appointments.Where(a => a.DoctorId == doctor.Id);
                }
                else
                {
                    appointments = appointments.Where(a => false);
                }
            }
            // Admin → no filter

            // =========================
            // 🔍 Search
            // =========================
            if (!string.IsNullOrEmpty(search))
            {
                appointments = appointments.Where(a =>
                    (a.Doctor != null && a.Doctor.Name.Contains(search)) ||
                    (a.Patient != null && (
                        a.Patient.Name.Contains(search) ||
                        a.Patient.Phone.Contains(search) // ✅ PHONE SEARCH
                    ))
                );
            }

            // =========================
            // 📌 Status filter
            // =========================
            if (status == "confirmed")
                appointments = appointments.Where(a => a.IsConfirmed);
            else if (status == "pending")
                appointments = appointments.Where(a => !a.IsConfirmed);

            // =========================
            // 📊 Sorting
            // =========================
            switch (sortOrder)
            {
                case "date_asc":
                    appointments = appointments.OrderBy(a => a.AppointmentDate);
                    break;
                default:
                    appointments = appointments.OrderByDescending(a => a.AppointmentDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(appointments.ToPagedList(pageNumber, pageSize));
        }

        // =========================
        // DETAILS (SECURED)
        // =========================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var appointment = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return HttpNotFound();

            // ✅ USER → only own
            if (User.IsInRole("User"))
            {
                var email = User.Identity.Name;

                if (appointment.Patient == null || appointment.Patient.Email != email)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }

            // ✅ DOCTOR → only own
            else if (Session["DoctorId"] != null)
            {
                int doctorId = (int)Session["DoctorId"];

                if (appointment.DoctorId != doctorId)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }
            else if (User.IsInRole("Doctor"))
            {
                var email = User.Identity.Name;
                var doctor = db.Doctors.FirstOrDefault(d => d.Email == email);

                if (doctor == null || appointment.DoctorId != doctor.Id)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }

            return View(appointment);
        }

        // =========================
        // CREATE GET
        // =========================
        public ActionResult Create(int? doctorId)
        {
            var model = new AppointmentCreateVM();

            if (doctorId.HasValue)
                model.DoctorId = doctorId.Value;

            ViewBag.DoctorId = new SelectList(db.Doctors, "Id", "Name", doctorId);

            return View(model);
        }

        // =========================
        // CREATE POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentCreateVM model)
        {
            if ((bool)!model.AgreeTerms)
            {
                ModelState.AddModelError("AgreeTerms", "You must agree to terms to proceed.");
            }

            if (ModelState.IsValid)
            {
                var patient = db.Patients.FirstOrDefault(p =>
                    p.Phone == model.Phone || p.Email == model.Email);

                if (patient == null)
                {
                    patient = new Patient
                    {
                        Name = model.PatientName,
                        Email = model.Email,
                        Phone = model.Phone,
                        Age = model.Age,
                        Gender = model.Gender
                    };
                    db.Patients.Add(patient);
                    db.SaveChanges();
                }

                string status = "Pending";
                bool isConfirmed = false;

                if (User.IsInRole("Admin") && !string.IsNullOrEmpty(model.Status))
                {
                    status = model.Status;
                    isConfirmed = status == "Confirmed";
                }

                var appointment = new Appointment
                {
                    DoctorId = model.DoctorId,
                    PatientId = patient.Id,
                    AppointmentDate = model.AppointmentDate,
                    Status = status,
                    IsConfirmed = isConfirmed
                };

                db.Appointments.Add(appointment);
                db.SaveChanges();

                TempData["Success"] = "Appointment created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.DoctorId = new SelectList(db.Doctors, "Id", "Name", model.DoctorId);
            return View(model);
        }

        // =========================
        // ADMIN ONLY ACTIONS
        // =========================
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var appointment = db.Appointments.Find(id);
            if (appointment == null)
                return HttpNotFound();

            ViewBag.DoctorId = new SelectList(db.Doctors, "Id", "Name", appointment.DoctorId);
            ViewBag.PatientId = new SelectList(db.Patients, "Id", "Name", appointment.PatientId);

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(appointment);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var appointment = db.Appointments.Find(id);
            if (appointment == null)
                return HttpNotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                db.Appointments.Remove(appointment);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Confirm(int id)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Confirmed";
                appointment.IsConfirmed = true;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // =========================
        // USER HISTORY
        // =========================
        [Authorize(Roles = "User")]
        public ActionResult MyAppointments(int? page)
        {
            var loginInput = User.Identity.Name;

            var appointments = db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Where(a =>
                    a.Patient.Phone == loginInput ||
                    a.Patient.Email == loginInput
                )
                .OrderByDescending(a => a.AppointmentDate);

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(appointments.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public JsonResult GetDoctorDetails(int doctorId)
        {
            var doctor = db.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefault(d => d.Id == doctorId);

            if (doctor == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                name = doctor.Name,
                specialization = doctor.Specialization != null ? doctor.Specialization.Name : "",
                experience = doctor.ExperienceYears + " Years",
                fee = doctor.ConsultationFee.ToString("C"),
                degree = doctor.Degree, // ✅ you asked for degree
                imagePath = string.IsNullOrEmpty(doctor.ImagePath)
                    ? Url.Content("~/Images/Doctors/default.png")
                    : Url.Content(doctor.ImagePath)
            }, JsonRequestBehavior.AllowGet);
        }


        // =========================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}