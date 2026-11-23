using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace DoctorAppointmentSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(ApplicationDbContext db) : base(db)
        {
        }

        // INDEX
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // DASHBOARD
        public IActionResult Dashboard()
        {
            var model = new DashboardVM();

            model.TotalDoctors = _db.Doctors.Count();
            model.TotalPatients = _db.Patients.Count();
            model.TotalAppointments = _db.Appointments.Count();
            model.TotalSpecializations = _db.Specializations.Count();

            DateTime today = DateTime.Today;

            model.TodayAppointments = _db.Appointments
                .Count(a => a.AppointmentDate.Date == today);

            var data = _db.Doctors
                .Include(d => d.Specialization)
                .Where(d => d.Specialization != null)
                .AsEnumerable()
                .GroupBy(d => d.Specialization.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Count = g.Count()
                })
                .ToList();

            model.SpecializationNames = data.Select(x => x.Name).ToList();
            model.DoctorCounts = data.Select(x => x.Count).ToList();

            var loginInput = User.Identity?.Name;
            var admin = _db.Users.FirstOrDefault(u => u.UserName == loginInput);

            if (admin != null)
            {
                model.UserName = admin.UserName;
            }
            else
            {
                model.UserName = loginInput;
            }

            ViewBag.UserName = model.UserName;
            return View(model);
        }
    }
}
