using System.ComponentModel.DataAnnotations;

namespace MMGC_Project.Models
{
    public class UpdateProfileDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Contact { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;
    }
}
