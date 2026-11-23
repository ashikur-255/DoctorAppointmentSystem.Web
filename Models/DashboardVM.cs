using System.Collections.Generic;

namespace DoctorAppointmentSystem.Models
{
    public class DashboardVM
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalSpecializations { get; set; }
        public int TodayAppointments { get; set; }

        public List<string> SpecializationNames { get; set; }
        public List<int> DoctorCounts { get; set; }

        // 🔥 ADD THIS
        public string UserName { get; set; }
    }
}