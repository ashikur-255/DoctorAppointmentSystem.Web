using DoctorAppointmentSystem.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.Controllers
{
    [Authorize]   // Only logged in users can access
    public class SpecializationsController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // =========================
        // GET: Specializations
        // =========================
        public ActionResult Index(string search, string sortOrder, int? page)
        {
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;

            var data = db.Specializations.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(s => s.Name.Contains(search));
            }

            switch (sortOrder)
            {
                case "name_asc":
                    data = data.OrderBy(s => s.Name);
                    break;

                case "name_desc":
                    data = data.OrderByDescending(s => s.Name);
                    break;

                default:
                    // Default → newest record first
                    data = data.OrderByDescending(s => s.Id);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(data.ToPagedList(pageNumber, pageSize));
        }


        // =========================
        // GET: Specializations/Details/5
        // =========================
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Specialization specialization = await db.Specializations.FindAsync(id);

            if (specialization == null)
                return HttpNotFound();

            return View(specialization);
        }


        // =========================
        // GET: Specializations/Create
        // =========================
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }


        // =========================
        // POST: Specializations/Create
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name")] Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                db.Specializations.Add(specialization);
                await db.SaveChangesAsync();

                TempData["Success"] = "Specialization added successfully!";
                return RedirectToAction("Index");
            }

            return View(specialization);
        }


        // =========================
        // GET: Specializations/Edit/5
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Specialization specialization = await db.Specializations.FindAsync(id);

            if (specialization == null)
                return HttpNotFound();

            return View(specialization);
        }


        // =========================
        // POST: Specializations/Edit/5
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                db.Entry(specialization).State = EntityState.Modified;
                await db.SaveChangesAsync();

                TempData["Success"] = "Specialization updated successfully!";
                return RedirectToAction("Index");
            }

            return View(specialization);
        }


        // =========================
        // GET: Specializations/Delete/5
        // =========================
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Specialization specialization = await db.Specializations.FindAsync(id);

            if (specialization == null)
                return HttpNotFound();

            return View(specialization);
        }


        // =========================
        // POST: Specializations/Delete/5
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Specialization specialization = await db.Specializations.FindAsync(id);

            if (specialization == null)
                return HttpNotFound();

            db.Specializations.Remove(specialization);
            await db.SaveChangesAsync();

            TempData["Success"] = "Specialization deleted successfully!";
            return RedirectToAction("Index");
        }


        // =========================
        // Dispose DB
        // =========================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}