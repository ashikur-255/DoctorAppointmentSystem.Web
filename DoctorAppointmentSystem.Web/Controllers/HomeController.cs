using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DoctorAppointmentSystem.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(ApplicationDbContext db) : base(db)
        {
        }

        public async Task<IActionResult> Index()
        {
            var doctors = await _db.Doctors
                .Include(d => d.Specialization)
                .ToListAsync();

            return View(doctors);
        }

        public async Task<IActionResult> About()
        {
            ViewBag.DoctorCount = await _db.Doctors.CountAsync();
            ViewBag.PatientCount = await _db.Patients.CountAsync();
            ViewBag.AppointmentCount = await _db.Appointments.CountAsync();
            ViewBag.SpecializationCount = await _db.Specializations.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult RefundPolicy()
        {
            return View();
        }

        public IActionResult Shipping()
        {
            return View();
        }

        public IActionResult Faq()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
