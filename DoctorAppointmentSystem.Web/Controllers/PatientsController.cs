using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using DoctorAppointmentSystem.Web.Helpers;

namespace DoctorAppointmentSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PatientsController : BaseController
    {
        public PatientsController(ApplicationDbContext db) : base(db)
        {
        }

        // INDEX (Search + Sort + Paging)
        public IActionResult Index(string search, string sortOrder, int? page)
        {
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSort = sortOrder == "name_desc" ? "" : "name_desc";

            var patients = _db.Patients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                patients = patients.Where(p =>
                    p.Name.Contains(search) ||
                    p.Phone.Contains(search) ||
                    p.Email.Contains(search) ||
                    p.DiseaseDetails.Contains(search));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    patients = patients.OrderByDescending(p => p.Name);
                    break;
                default:
                    patients = patients.OrderBy(p => p.Name);
                    break;
            }

            int pageSize = 15;
            int pageNumber = page ?? 1;

            return View(patients.ToPagedList(pageNumber, pageSize));
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var patient = await _db.Patients
                .Include(p => p.Appointments)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        // CREATE
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Phone,Email,Age,Gender,BloodGroup,Address,DiseaseDetails")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _db.Patients.Add(patient);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Patient created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(patient);
        }

        // EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var patient = await _db.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Phone,Email,Age,Gender,BloodGroup,Address,DiseaseDetails")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _db.Update(patient);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Patient updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(patient);
        }

        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var patient = await _db.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient != null)
            {
                _db.Patients.Remove(patient);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
