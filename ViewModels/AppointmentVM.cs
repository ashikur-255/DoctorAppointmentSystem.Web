using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.ViewModels
{
    public class AppointmentVM
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; }

        public IEnumerable<SelectListItem> Doctors { get; set; }
        public IEnumerable<SelectListItem> Patients { get; set; }


    }
}