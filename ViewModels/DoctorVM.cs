using DoctorAppointmentSystem.Models;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace DoctorAppointmentSystem.ViewModels
{
    public class DoctorVM
    {
        public Doctor Doctor { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }

        public IEnumerable<SelectListItem> Specializations { get; set; }
    }
}