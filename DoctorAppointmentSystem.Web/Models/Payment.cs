using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentSystem.Web.Models
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
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public bool IsPaid { get; set; } = false;

        public string TransactionId { get; set; }

        public string Status { get; set; } = "Pending";

        public virtual Appointment Appointment { get; set; }
    }
}
