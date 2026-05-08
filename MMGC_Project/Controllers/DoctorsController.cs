using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMGC_Project.Data;
using MMGC_Project.Models;

namespace MMGC_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── POST /api/doctors ────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DoctorDto dto)
        {
            var doctor = new Doctor
            {
                Name              = dto.Name,
                Specialization    = dto.Specialization,
                TotalAppointments = dto.TotalAppointments,
                TotalRevenue      = dto.TotalRevenue,
                Status            = dto.Status,
                CreatedAt         = DateTime.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status  = "Success",
                Message = "Doctor created successfully.",
                Doctor  = doctor
            });
        }

        // ── GET /api/doctors/all-doctors (Unprotected) ──────────────────────
        [HttpGet("all-doctors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _context.Doctors
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                Status  = "Success",
                Count   = doctors.Count,
                Doctors = doctors
            });
        }

        // ── GET /api/doctors/{id} ─────────────────────────────────────────────
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound(new { Status = "Error", Message = $"Doctor with ID {id} not found." });

            return Ok(new { Status = "Success", Doctor = doctor });
        }

        // ── PUT /api/doctors/{id} ────────────────────────────────────────────
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DoctorDto dto)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound(new { Status = "Error", Message = $"Doctor with ID {id} not found." });

            doctor.Name              = dto.Name;
            doctor.Specialization    = dto.Specialization;
            doctor.TotalAppointments = dto.TotalAppointments;
            doctor.TotalRevenue      = dto.TotalRevenue;
            doctor.Status            = dto.Status;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status  = "Success",
                Message = "Doctor updated successfully.",
                Doctor  = doctor
            });
        }

        // ── DELETE /api/doctors/{id} ─────────────────────────────────────────
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
                return NotFound(new { Status = "Error", Message = $"Doctor with ID {id} not found." });

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = $"Doctor with ID {id} deleted successfully." });
        }
    }
}
