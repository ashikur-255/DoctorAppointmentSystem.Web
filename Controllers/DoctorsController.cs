using DoctorAppointmentSystem.Models;
using PagedList;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    
    public class DoctorsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =============================
        // INDEX (Search + Sort + Paging)
        // =============================
        
        public ActionResult Index(string search, string status, int? specializationId, string sortOrder, int? page)
        {
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSpecialization = specializationId;

            ViewBag.Specializations = new SelectList(db.Specializations, "Id", "Name", specializationId);

            var doctors = db.Doctors.Include(d => d.Specialization).AsQueryable();

            // 🔎 Search
            if (!string.IsNullOrEmpty(search))
            {
                doctors = doctors.Where(d =>
                    d.Name.Contains(search) ||
                    d.Specialization.Name.Contains(search));
            }

            // 🎯 Status Filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Available")
                    doctors = doctors.Where(d => d.IsAvailable);
                else if (status == "Not Available")
                    doctors = doctors.Where(d => !d.IsAvailable);
            }

            // 🎯 Specialization Filter
            if (specializationId != null)
            {
                doctors = doctors.Where(d => d.SpecializationId == specializationId);
            }

            // ↕ Sorting
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

        // =============================
        // DETAILS
        // =============================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var doctor = db.Doctors.Include(d => d.Specialization).FirstOrDefault(d => d.Id == id);
            if (doctor == null)
                return HttpNotFound();

            return View(doctor);
        }
        
        // =============================
        // CREATE
        // =============================
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.SpecializationId = new SelectList(db.Specializations, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Doctor doctor, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                // Save Image
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string folder = Server.MapPath("~/Images/Doctors/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fullPath = Path.Combine(folder, fileName);
                    imageFile.SaveAs(fullPath);
                    doctor.ImagePath = "/Images/Doctors/" + fileName;
                }

                db.Doctors.Add(doctor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SpecializationId = new SelectList(db.Specializations, "Id", "Name", doctor.SpecializationId);
            return View(doctor);
        }

        // =============================
        // EDIT
        // =============================
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Doctor doctor = db.Doctors.Find(id);
            if (doctor == null)
                return HttpNotFound();

            ViewBag.SpecializationId = new SelectList(db.Specializations, "Id", "Name", doctor.SpecializationId);
            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Doctor doctor, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var existingDoctor = db.Doctors.Find(doctor.Id);
                if (existingDoctor == null)
                    return HttpNotFound();

                // Update fields
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

                // Update Image
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                    string folder = Server.MapPath("~/Images/Doctors/");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string fullPath = Path.Combine(folder, fileName);
                    imageFile.SaveAs(fullPath);
                    existingDoctor.ImagePath = "/Images/Doctors/" + fileName;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.SpecializationId = new SelectList(db.Specializations, "Id", "Name", doctor.SpecializationId);
            return View(doctor);
        }

        // =============================
        // DELETE
        // =============================
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var doctor = db.Doctors.Include(d => d.Specialization).FirstOrDefault(d => d.Id == id);
            if (doctor == null)
                return HttpNotFound();

            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var doctor = db.Doctors.Find(id);
            if (doctor != null)
            {
                db.Doctors.Remove(doctor);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // =============================
        // DISPOSE
        // =============================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}