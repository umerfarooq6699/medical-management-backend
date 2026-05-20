using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MMGC_Project.Data;
using MMGC_Project.Models;

namespace MMGC_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ── POST /api/appointments ───────────────────────────────────────────────
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] AppointmentDto dto)
        {
            var appointment = new Appointment
            {
                PatientName         = dto.PatientName,
                DoctorName          = dto.DoctorName,
                NurseName           = dto.NurseName,
                AppointmentDateTime = dto.AppointmentDateTime,
                Status              = dto.Status,
                PhoneNumber         = dto.PhoneNumber,
                Symptoms            = dto.Symptoms,
                TimeSlot            = dto.TimeSlot,
                CreatedAt           = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, new
            {
                Status      = "Success",
                Message     = "Appointment created successfully.",
                Appointment = appointment
            });
        }

        // ── GET /api/appointments/all-appointments (Unprotected) ────────────────
        [HttpGet("all-appointments")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _context.Appointments
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            return Ok(new
            {
                Status       = "Success",
                Count        = appointments.Count,
                Appointments = appointments
            });
        }


        // ── GET /api/appointments/{id} ───────────────────────────────────────────
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { Status = "Error", Message = $"Appointment with ID {id} not found." });

            return Ok(new { Status = "Success", Appointment = appointment });
        }

        // ── PUT /api/appointments/{id} ────────────────────────────────────────────
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Update(int id, [FromBody] AppointmentDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { Status = "Error", Message = $"Appointment with ID {id} not found." });

            appointment.PatientName         = dto.PatientName;
            appointment.DoctorName          = dto.DoctorName;
            appointment.NurseName           = dto.NurseName;
            appointment.AppointmentDateTime = dto.AppointmentDateTime;
            appointment.Status              = dto.Status;
            appointment.PhoneNumber         = dto.PhoneNumber;
            appointment.Symptoms            = dto.Symptoms;
            appointment.TimeSlot            = dto.TimeSlot;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Status      = "Success",
                Message     = "Appointment updated successfully.",
                Appointment = appointment
            });
        }

        // ── DELETE /api/appointments/{id} ─────────────────────────────────────────
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { Status = "Error", Message = $"Appointment with ID {id} not found." });

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = $"Appointment with ID {id} deleted successfully." });
        }
    }
}
