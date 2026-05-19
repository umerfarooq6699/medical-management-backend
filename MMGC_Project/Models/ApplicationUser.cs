using Microsoft.AspNetCore.Identity;

namespace MMGC_Project.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Role { get; set; } = "patient";
    }
}
