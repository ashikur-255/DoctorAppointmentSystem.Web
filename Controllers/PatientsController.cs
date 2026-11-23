using DoctorAppointmentSystem.Models;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PatientsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // =============================
        // INDEX (Search + Sort + Paging)
        // =============================
        public ActionResult Index(string search, string sortOrder, int? page)
        {
            // Current search and sort for view
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSort = sortOrder == "name_desc" ? "" : "name_desc";

            // Base query
            var patients = db.Patients.AsNoTracking().AsQueryable();

            // 🔎 Search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim(); // remove extra spaces
                patients = patients.Where(p =>
                    p.Name.Contains(search) ||
                    p.Phone.Contains(search) ||
                    p.Email.Contains(search) ||
                    p.DiseaseDetails.Contains(search));
            }

            // ↕ Sorting
            switch (sortOrder)
            {
                case "name_desc":
                    patients = patients.OrderByDescending(p => p.Name);
                    break;
                default:
                    patients = patients.OrderBy(p => p.Name);
                    break;
            }

            // 🧮 Pagination
            int pageSize = 15;
            int pageNumber = page ?? 1;

            // Return view with PagedList
            return View(patients.ToPagedList(pageNumber, pageSize));
        }

        // =============================
        // DETAILS
        // =============================
        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var patient = db.Patients
                            .Include(p => p.Appointments)
                            .AsNoTracking()
                            .FirstOrDefault(p => p.Id == id);

            if (patient == null)
                return HttpNotFound();

            return View(patient);
        }

        // =============================
        // CREATE
        // =============================
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
            "Name,Phone,Email,Age,Gender,BloodGroup,Address,DiseaseDetails")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                db.Patients.Add(patient);
                db.SaveChanges();

                TempData["Success"] = "Patient created successfully!";
                return RedirectToAction("Index");
            }

            return View(patient);
        }

        // =============================
        // EDIT
        // =============================
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Patient patient = db.Patients.Find(id);

            if (patient == null)
                return HttpNotFound();

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include =
            "Id,Name,Phone,Email,Age,Gender,BloodGroup,Address,DiseaseDetails")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                db.Entry(patient).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Success"] = "Patient updated successfully!";
                return RedirectToAction("Index");
            }

            return View(patient);
        }

        // =============================
        // DELETE
        // =============================
        [Authorize(Roles = "Admin")]
        // GET: Patients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return HttpNotFound();

            Patient patient = db.Patients.Find(id);

            if (patient == null)
                return HttpNotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Patient patient = db.Patients.Find(id);

            db.Patients.Remove(patient);

            db.SaveChanges();

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