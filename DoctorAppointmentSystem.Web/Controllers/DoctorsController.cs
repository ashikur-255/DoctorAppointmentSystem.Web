using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DoctorAppointmentSystem.Web.Helpers;

namespace DoctorAppointmentSystem.Web.Controllers
{
    public class DoctorsController : BaseController
    {
        private readonly IWebHostEnvironment _env;

        public DoctorsController(ApplicationDbContext db, IWebHostEnvironment env) : base(db)
        {
            _env = env;
        }

        // INDEX (Search + Sort + Paging)
        public IActionResult Index(string search, string status, int? specializationId, string sortOrder, int? page)
        {
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSpecialization = specializationId;

            ViewBag.Specializations = new SelectList(_db.Specializations.ToList(), "Id", "Name", specializationId);

            var doctors = _db.Doctors.Include(d => d.Specialization).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                doctors = doctors.Where(d =>
                    d.Name.Contains(search) ||
                    d.Specialization.Name.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Available")
                    doctors = doctors.Where(d => d.IsAvailable);
                else if (status == "Not Available")
                    doctors = doctors.Where(d => !d.IsAvailable);
            }

            if (specializationId != null)
            {
                doctors = doctors.Where(d => d.SpecializationId == specializationId);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    doctors = doctors.OrderByDescending(d => d.Name);
                    break;
                case "exp":
                    doctors = doctors.OrderBy(d => d.ExperienceYears);
                    break;
                case "exp_desc":
                    doctors = doctors.OrderByDescending(d => d.ExperienceYears);
                    break;
                case "fee":
                    doctors = doctors.OrderBy(d => d.ConsultationFee);
                    break;
                case "fee_desc":
                    doctors = doctors.OrderByDescending(d => d.ConsultationFee);
                    break;
                case "date":
                    doctors = doctors.OrderBy(d => d.JoiningDate);
                    break;
                case "date_desc":
                    doctors = doctors.OrderByDescending(d => d.JoiningDate);
                    break;
                default:
                    doctors = doctors.OrderBy(d => d.Name);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;
            return View(doctors.ToPagedList(pageNumber, pageSize));
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var doctor = await _db.Doctors.Include(d => d.Specialization).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        // CREATE
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.SpecializationId = new SelectList(_db.Specializations.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Doctor doctor, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(ms);
                        var bytes = ms.ToArray();
                        var base64 = Convert.ToBase64String(bytes);
                        var contentType = string.IsNullOrEmpty(imageFile.ContentType) ? "image/jpeg" : imageFile.ContentType;
                        doctor.ImagePath = $"data:{contentType};base64,{base64}";

                        try
                        {
                            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                            string folder = Path.Combine(webRoot, "Images", "Doctors");
                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);

                            string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                            string fullPath = Path.Combine(folder, fileName);
                            await System.IO.File.WriteAllBytesAsync(fullPath, bytes);
                        }
                        catch { }
                    }
                }
                else
                {
                    doctor.ImagePath = "/Images/Doctors/default.png";
                }

                _db.Doctors.Add(doctor);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SpecializationId = new SelectList(_db.Specializations.ToList(), "Id", "Name", doctor.SpecializationId);
            return View(doctor);
        }

        // EDIT
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            ViewBag.SpecializationId = new SelectList(_db.Specializations.ToList(), "Id", "Name", doctor.SpecializationId);
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Doctor doctor, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingDoctor = await _db.Doctors.FindAsync(doctor.Id);
                if (existingDoctor == null)
                    return NotFound();

                existingDoctor.Name = doctor.Name;
                existingDoctor.HospitalName = doctor.HospitalName;
                existingDoctor.RegistrationNo = doctor.RegistrationNo;
                existingDoctor.Degree = doctor.Degree;
                existingDoctor.VisitedTime = doctor.VisitedTime;
                existingDoctor.ExperienceYears = doctor.ExperienceYears;
                existingDoctor.ConsultationFee = doctor.ConsultationFee;
                existingDoctor.IsAvailable = doctor.IsAvailable;
                existingDoctor.JoiningDate = doctor.JoiningDate;
                existingDoctor.SpecializationId = doctor.SpecializationId;

                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(ms);
                        var bytes = ms.ToArray();
                        var base64 = Convert.ToBase64String(bytes);
                        var contentType = string.IsNullOrEmpty(imageFile.ContentType) ? "image/jpeg" : imageFile.ContentType;
                        existingDoctor.ImagePath = $"data:{contentType};base64,{base64}";

                        try
                        {
                            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                            string folder = Path.Combine(webRoot, "Images", "Doctors");
                            if (!Directory.Exists(folder))
                                Directory.CreateDirectory(folder);

                            string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                            string fullPath = Path.Combine(folder, fileName);
                            await System.IO.File.WriteAllBytesAsync(fullPath, bytes);
                        }
                        catch { }
                    }
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SpecializationId = new SelectList(_db.Specializations.ToList(), "Id", "Name", doctor.SpecializationId);
            return View(doctor);
        }

        // DELETE
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var doctor = await _db.Doctors.Include(d => d.Specialization).FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _db.Doctors.Remove(doctor);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
