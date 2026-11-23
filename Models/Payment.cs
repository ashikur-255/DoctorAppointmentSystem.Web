using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public bool IsPaid { get; set; } = false;

        // 🔥 NEW FIELDS
        public string TransactionId { get; set; }

        public string Status { get; set; } = "Pending";
        // Pending / Paid / Failed

        public virtual Appointment Appointment { get; set; }
    }
}