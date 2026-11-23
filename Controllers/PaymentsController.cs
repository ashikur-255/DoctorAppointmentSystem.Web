using DoctorAppointmentSystem.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace DoctorAppointmentSystem.Controllers
{
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ================= CREATE =================
        public ActionResult Create(int appointmentId)
        {
            var appointment = db.Appointments
                .Include("Doctor") // 🔥 FIX
                .FirstOrDefault(a => a.Id == appointmentId);

            if (appointment == null)
                return HttpNotFound();

            var existingPayment = db.Payments
                .FirstOrDefault(p => p.AppointmentId == appointmentId && p.IsPaid);

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

        // ================= POST CREATE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Payment payment)
        {
            try
            {
                var appointment = db.Appointments
                    .Include("Doctor") // 🔥 FIX
                    .FirstOrDefault(a => a.Id == payment.AppointmentId);

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

                db.Payments.Add(payment);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Payment submitted successfully!",
                    redirectUrl = Url.Action("Index", "Appointments") // 🔥 FIX
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Verify(int id)
        {
            var payment = db.Payments.Find(id);
            if (payment == null) return HttpNotFound();

            payment.Status = "Paid";
            payment.IsPaid = true;

            db.SaveChanges();

            return RedirectToAction("Index", "Appointments");
        }
    }
}