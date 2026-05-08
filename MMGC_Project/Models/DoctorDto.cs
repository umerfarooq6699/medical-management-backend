namespace MMGC_Project.Models
{
    public class DoctorDto
    {
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int TotalAppointments { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;
        public string Status { get; set; } = "Active";
    }
}
