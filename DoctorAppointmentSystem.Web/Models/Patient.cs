using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.Web.Models
{
    public class Patient
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Patient Name")]
        public string Name { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Age is required")]
        public int? Age { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [Display(Name = "Disease Details")]
        [StringLength(500)]
        public string DiseaseDetails { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
