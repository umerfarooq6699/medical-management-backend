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
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Patient";

        [Required]
        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;
    }
}
