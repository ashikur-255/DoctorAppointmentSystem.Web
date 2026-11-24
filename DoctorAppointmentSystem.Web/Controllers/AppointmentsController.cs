using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using DoctorAppointmentSystem.Web.Helpers;

namespace DoctorAppointmentSystem.Web.Controllers
{
    [Authorize]
    public class AppointmentsController : BaseController
    {
        public AppointmentsController(ApplicationDbContext db) : base(db)
        {
        }

        // INDEX
        public IActionResult Index(string search, string status, string sortOrder, int? page)
        {
            if (User.IsInRole("User"))
            {
                return RedirectToAction(nameof(MyAppointments));
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSort = sortOrder == "date_desc" ? "date_asc" : "date_desc";

            var appointments = _db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .AsQueryable();

            var doctorIdSession = HttpContext.Session.GetInt32("DoctorId");
            if (doctorIdSession != null)
            {
                int docId = doctorIdSession.Value;
                appointments = appointments.Where(a => a.DoctorId == docId);
            }
            else if (User.IsInRole("Doctor"))
            {
                var email = User.Identity.Name;
                var doctor = _db.Doctors.FirstOrDefault(d => d.Email == email);

                if (doctor != null)
                {
                    appointments = appointments.Where(a => a.DoctorId == doctor.Id);
                }
                else
                {
                    appointments = appointments.Where(a => false);
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                appointments = appointments.Where(a =>
                    (a.Doctor != null && a.Doctor.Name.Contains(search)) ||
                    (a.Patient != null && (
                        a.Patient.Name.Contains(search) ||
                        a.Patient.Phone.Contains(search)
                    ))
                );
            }

            if (status == "confirmed")
                appointments = appointments.Where(a => a.IsConfirmed);
            else if (status == "pending")
                appointments = appointments.Where(a => !a.IsConfirmed);

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

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var appointment = await _db.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            if (User.IsInRole("User"))
            {
                var email = User.Identity.Name;
                if (appointment.Patient == null || (appointment.Patient.Email != email && appointment.Patient.Phone != email))
                {
                    return Forbid();
                }
            }
            else if (HttpContext.Session.GetInt32("DoctorId") != null)
            {
                int docId = HttpContext.Session.GetInt32("DoctorId").Value;
                if (appointment.DoctorId != docId)
                {
                    return Forbid();
                }
            }
            else if (User.IsInRole("Doctor"))
            {
                var email = User.Identity.Name;
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Email == email);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                {
                    return Forbid();
                }
            }

            return View(appointment);
        }

        // CREATE GET
        [AllowAnonymous]
        public IActionResult Create(int? doctorId)
        {
            var model = new AppointmentCreateVM();

            if (doctorId.HasValue)
                model.DoctorId = doctorId.Value;

            ViewBag.DoctorId = new SelectList(_db.Doctors.ToList(), "Id", "Name", doctorId);

            return View(model);
        }

        // CREATE POST
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateVM model)
        {
            if (!model.AgreeTerms)
            {
                ModelState.AddModelError("AgreeTerms", "You must agree to terms to proceed.");
            }

            if (ModelState.IsValid)
            {
                var patient = await _db.Patients.FirstOrDefaultAsync(p =>
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
                    _db.Patients.Add(patient);
                    await _db.SaveChangesAsync();
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

                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Appointment created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DoctorId = new SelectList(_db.Doctors.ToList(), "Id", "Name", model.DoctorId);
            return View(model);
        }

        // EDIT
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            ViewBag.DoctorId = new SelectList(_db.Doctors.ToList(), "Id", "Name", appointment.DoctorId);
            ViewBag.PatientId = new SelectList(_db.Patients.ToList(), "Id", "Name", appointment.PatientId);

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _db.Update(appointment);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(appointment);
        }

        // DELETE
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _db.Appointments.Remove(appointment);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // CONFIRM
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = "Confirmed";
                appointment.IsConfirmed = true;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // USER HISTORY
        [Authorize(Roles = "User")]
        public IActionResult MyAppointments(int? page)
        {
            var loginInput = User.Identity?.Name;

            var appointments = _db.Appointments
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
        public async Task<IActionResult> GetDoctorDetails(int doctorId)
        {
            var doctor = await _db.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                name = doctor.Name,
                specialization = doctor.Specialization != null ? doctor.Specialization.Name : "",
                experience = doctor.ExperienceYears + " Years",
                fee = "৳ " + doctor.ConsultationFee.ToString("N2"),
                degree = doctor.Degree,
                imagePath = string.IsNullOrEmpty(doctor.ImagePath)
                    ? Url.Content("~/Images/Doctors/default.png")
                    : Url.Content(doctor.ImagePath)
            });
        }
    }
}
