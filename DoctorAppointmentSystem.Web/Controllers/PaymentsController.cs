using DoctorAppointmentSystem.Web.Data;
using DoctorAppointmentSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DoctorAppointmentSystem.Web.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PaymentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // CREATE
        public async Task<IActionResult> Create(int appointmentId)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
                return NotFound();

            var existingPayment = await _db.Payments
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.IsPaid);

            if (existingPayment != null)
            {
                TempData["Error"] = "Payment already completed!";
                return RedirectToAction("Details", "Appointments", new { id = appointmentId });
            }

            var model = new Payment
            {
                AppointmentId = appointmentId,
                Amount = appointment.Doctor?.ConsultationFee ?? 0
            };

            return View(model);
        }

        // POST CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            try
            {
                var appointment = await _db.Appointments
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.Id == payment.AppointmentId);

                if (appointment == null)
                    return Json(new { success = false, message = "Invalid appointment!" });

                decimal actualAmount = appointment.Doctor?.ConsultationFee ?? 0;

                if (payment.Amount != actualAmount)
                    return Json(new { success = false, message = "Invalid amount!" });

                if (payment.PaymentMethod != "Cash" && string.IsNullOrEmpty(payment.TransactionId))
                    return Json(new { success = false, message = "Transaction ID required!" });

                payment.PaymentDate = DateTime.Now;
                payment.Status = "Pending";
                payment.IsPaid = false;

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Payment submitted successfully!",
                    redirectUrl = Url.Action("Index", "Appointments")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Verify(int id)
        {
            var payment = await _db.Payments.FindAsync(id);
            if (payment == null)
                return NotFound();

            payment.Status = "Paid";
            payment.IsPaid = true;

            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Appointments");
        }
    }
}
