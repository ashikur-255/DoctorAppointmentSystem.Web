using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DoctorAppointmentSystem.Web.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorPortalController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public DoctorPortalController(ApplicationDbContext db, SignInManager<ApplicationUser> signInManager) : base(db)
        {
            _signInManager = signInManager;
        }

        // STEP 1: ENTER REGISTRATION NO
        public async Task<IActionResult> SelectDoctor()
        {
            ViewBag.Doctors = await _db.Doctors.Include(d => d.Specialization).OrderBy(d => d.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectDoctor(string regNo)
        {
            regNo = regNo?.Trim();
            if (string.IsNullOrEmpty(regNo))
            {
                ViewBag.Error = "Registration Number is required!";
                ViewBag.Doctors = await _db.Doctors.Include(d => d.Specialization).OrderBy(d => d.Name).ToListAsync();
                return View();
            }

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.RegistrationNo.ToLower() == regNo.ToLower());

            if (doctor == null)
            {
                ViewBag.Error = $"Invalid Registration Number '{regNo}'. Please select from the list below.";
                ViewBag.Doctors = await _db.Doctors.Include(d => d.Specialization).OrderBy(d => d.Name).ToListAsync();
                return View();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (doctor.UserId == null && currentUserId != null)
            {
                doctor.UserId = currentUserId;
                await _db.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32("DoctorId", doctor.Id);
            HttpContext.Session.SetString("DoctorName", doctor.Name);

            return RedirectToAction(nameof(Dashboard));
        }

        // DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
                return RedirectToAction(nameof(SelectDoctor));

            DateTime today = DateTime.Today;

            var appointments = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate.Date == today &&
                            a.IsConfirmed && !a.IsVisited)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.TotalPatients = appointments.Count;
            ViewBag.DoctorName = HttpContext.Session.GetString("DoctorName");

            return View(appointments);
        }

        // HISTORY
        public async Task<IActionResult> History()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
                return RedirectToAction(nameof(SelectDoctor));

            var history = await _db.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId && a.IsVisited)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(history);
        }

        // PROFILE
        public async Task<IActionResult> Profile()
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
                return RedirectToAction(nameof(SelectDoctor));

            var doctor = await _db.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // PRESCRIPTION
        public async Task<IActionResult> Prescription(int id)
        {
            var doctorId = HttpContext.Session.GetInt32("DoctorId");
            if (doctorId == null)
                return RedirectToAction(nameof(SelectDoctor));

            var appointment = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id && a.DoctorId == doctorId);

            if (appointment == null)
                return NotFound();

            return View(appointment);
        }

        // MARK VISITED
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkVisited(int id)
        {
            var app = await _db.Appointments.FindAsync(id);
            if (app == null)
                return Json(new { success = false, message = "Appointment not found!" });

            app.IsVisited = true;
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // LOGOUT (SWITCH DOCTOR / CLEAR CURRENT DOCTOR SESSION)
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("DoctorId");
            HttpContext.Session.Remove("DoctorName");
            return RedirectToAction(nameof(SelectDoctor));
        }
    }
}
