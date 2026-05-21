using Microsoft.AspNetCore.Mvc;
using MMGC_Project.Data;
using MMGC_Project.Models;
using System;
using System.Threading.Tasks;

namespace MMGC_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitMessage([FromBody] ContactMessageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contactMessage = new ContactMessage
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Subject = dto.Subject,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message saved successfully." });
        }
    }
}
