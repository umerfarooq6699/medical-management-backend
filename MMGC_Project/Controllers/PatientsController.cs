using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMGC_Project.Data;
using MMGC_Project.Models;

namespace MMGC_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── POST /api/patients ───────────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] PatientDto dto)
        {
            var patient = new Patient
            {
                Name      = dto.Name,
                Age       = dto.Age,
                Gender    = dto.Gender,
                Contact   = dto.Contact,
                Address   = dto.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = patient.Id }, new
            {
                Status  = "Success",
                Message = "Patient created successfully.",
                Patient = patient
            });
        }

        // ── GET /api/patients ────────────────────────────────────────────────
        [HttpGet("all-patients")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _context.Patients
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                Status  = "Success",
                Count   = patients.Count,
                Patients = patients
            });
        }

        // ── GET /api/patients/{id} ───────────────────────────────────────────
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(new { Status = "Error", Message = $"Patient with ID {id} not found." });

            return Ok(new { Status = "Success", Patient = patient });
        }

        // ── PUT /api/patients/{id} ────────────────────────────────────────────
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int id, [FromBody] PatientDto dto)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(new { Status = "Error", Message = $"Patient with ID {id} not found." });

            // Update fields
            patient.Name    = dto.Name;
            patient.Age     = dto.Age;
            patient.Gender  = dto.Gender;
            patient.Contact = dto.Contact;
            patient.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status  = "Success",
                Message = "Patient updated successfully.",
                Patient = patient
            });
        }

        // ── DELETE /api/patients/{id} ─────────────────────────────────────────
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _context.Patients.FindAsync(id);

            if (patient == null)
                return NotFound(new { Status = "Error", Message = $"Patient with ID {id} not found." });

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = $"Patient with ID {id} deleted successfully." });
        }
    }
}
