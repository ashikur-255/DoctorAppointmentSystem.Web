using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentSystem.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Confirmed")]
        public bool IsConfirmed { get; set; }
        public string Status { get; set; }

        // Foreign Keys
        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Patient")]
        public int PatientId { get; set; }

        // Navigation Properties
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        public bool IsVisited { get; set; }


    }
}