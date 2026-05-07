using System;

namespace MMGC_Project.Models
{
    public class AppointmentDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string NurseName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
    }
}
