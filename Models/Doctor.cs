using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointmentSystem.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Doctor Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Doctor Name")]
        public string Name { get; set; }

        public string UserId { get; set; } // 🔥 MUST

        [Required(ErrorMessage = "Hospital Name is required")]
        [StringLength(200, ErrorMessage = "Hospital Name cannot exceed 200 characters")]
        [Display(Name = "Hospital Name")]
        public string HospitalName { get; set; }

        [Required(ErrorMessage = "Registration Number is required")]
        [StringLength(50, ErrorMessage = "Registration Number cannot exceed 50 characters")]
        [Display(Name = "Registration Number")]
        public string RegistrationNo { get; set; }

        [Required(ErrorMessage = "Degree is required")]
        [StringLength(100, ErrorMessage = "Degree cannot exceed 100 characters")]
        [Display(Name = "Degree")]
        public string Degree { get; set; }

        [Display(Name = "Visited Time")]
        [DataType(DataType.Time)]
        public TimeSpan? VisitedTime { get; set; }   // <-- Use TimeSpan? for time-only

        [Range(0, 50, ErrorMessage = "Experience must be between 0 and 50 years")]
        [Display(Name = "Years of Experience")]
        public int ExperienceYears { get; set; }

        [Range(0, 10000, ErrorMessage = "Fee must be positive")]
        [DataType(DataType.Currency)]
        [Display(Name = "Consultation Fee")]
        public decimal ConsultationFee { get; set; }

        [Display(Name = "Available for Appointment")]
        public bool IsAvailable { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Doctor Photo")]
        public string ImagePath { get; set; }

        [Display(Name = "Specialization")]
        public int SpecializationId { get; set; }

        [ForeignKey("SpecializationId")]
        public virtual Specialization Specialization { get; set; }
        public string Email { get; internal set; }
        //public int Fee { get; internal set; }
        //public string Experience { get; internal set; }
    }
}