using System.ComponentModel.DataAnnotations;

namespace MMGC_Project.Models
{
    public class RegisterDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Admin, Doctor, Patient, etc.

        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;
    }
}
