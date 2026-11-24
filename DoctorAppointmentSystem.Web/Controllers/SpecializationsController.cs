using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoctorAppointmentSystem.Web.Helpers;

namespace DoctorAppointmentSystem.Web.Controllers
{
    [Authorize]
    public class SpecializationsController : BaseController
    {
        public SpecializationsController(ApplicationDbContext db) : base(db)
        {
        }

        // GET: Specializations
        [AllowAnonymous]
        public IActionResult Index(string search, string sortOrder, int? page)
        {
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;

            var data = _db.Specializations.AsQueryable();

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
                    data = data.OrderByDescending(s => s.Id);
                    break;
            }

            int pageSize = 10;
            int pageNumber = page ?? 1;

            return View(data.ToPagedList(pageNumber, pageSize));
        }

        // GET: Specializations/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var specialization = await _db.Specializations.FindAsync(id);
            if (specialization == null)
                return NotFound();

            return View(specialization);
        }

        // GET: Specializations/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Specializations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Name")] Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                _db.Specializations.Add(specialization);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Specialization added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(specialization);
        }

        // GET: Specializations/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var specialization = await _db.Specializations.FindAsync(id);
            if (specialization == null)
                return NotFound();

            return View(specialization);
        }

        // POST: Specializations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit([Bind("Id,Name")] Specialization specialization)
        {
            if (ModelState.IsValid)
            {
                _db.Update(specialization);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Specialization updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(specialization);
        }

        // GET: Specializations/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var specialization = await _db.Specializations.FindAsync(id);
            if (specialization == null)
                return NotFound();

            return View(specialization);
        }

        // POST: Specializations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var specialization = await _db.Specializations.FindAsync(id);
            if (specialization == null)
                return NotFound();

            _db.Specializations.Remove(specialization);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Specialization deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
