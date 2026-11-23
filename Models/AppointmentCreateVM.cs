using System;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.Models
{
    public class AppointmentCreateVM
    {
        [Required(ErrorMessage = "Please select a doctor.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Please select an appointment date.")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Patient name is required.")]
        public string PatientName { get; set; }

        // Admin-only status; default to Pending
        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Age is required.")]
        [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
        public int? Age { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        public string Notes { get; set; }

        [Required(ErrorMessage = "You must agree to Terms")]
        public bool AgreeTerms { get; set; }
    }
}
