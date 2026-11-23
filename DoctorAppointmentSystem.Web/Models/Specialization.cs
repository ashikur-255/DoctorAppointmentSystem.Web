using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoctorAppointmentSystem.Web.Models
{
    public class Specialization
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
