using System;

namespace MMGC_Project.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string NurseName { get; set; } = string.Empty;
        public DateTime AppointmentDateTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked; // Default status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
