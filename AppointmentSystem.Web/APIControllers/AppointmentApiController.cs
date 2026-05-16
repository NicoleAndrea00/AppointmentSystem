using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;
using MediBook.Models;

namespace MediBook.ApiControllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly AppointmentDbContext _context;

        public AppointmentsApiController(AppointmentDbContext context)
        {
            _context = context;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes,
                    a.ClinicalNotes,
                    Patient = new { a.Patient.Id, a.Patient.FullName, a.Patient.Email },
                    Clinician = new { a.Clinician.Id, a.Clinician.User.FullName, a.Clinician.Speciality }
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/appointments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes,
                    a.ClinicalNotes,
                    Patient = new { a.Patient.Id, a.Patient.FullName, a.Patient.Email },
                    Clinician = new { a.Clinician.Id, a.Clinician.User.FullName, a.Clinician.Speciality }
                })
                .FirstOrDefaultAsync();

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            return Ok(appointment);
        }

        // GET: api/appointments/patient/5
        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .Where(a => a.PatientId == patientId)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes,
                    Clinician = new { a.Clinician.User.FullName, a.Clinician.Speciality }
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/appointments/clinician/5
        [HttpGet("clinician/{clinicianId}")]
        public async Task<IActionResult> GetByClinician(int clinicianId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.ClinicianId == clinicianId)
                .Select(a => new
                {
                    a.Id,
                    a.AppointmentDate,
                    a.Status,
                    a.Notes,
                    Patient = new { a.Patient.FullName, a.Patient.Email }
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Appointment model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.CreatedAt = DateTime.UtcNow;
            model.Status = "Scheduled";

            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        // PUT: api/appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Appointment model)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            appointment.AppointmentDate = model.AppointmentDate;
            appointment.Status = model.Status;
            appointment.Notes = model.Notes;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment updated successfully" });
        }

        // DELETE: api/appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Appointment deleted successfully" });
        }
    }
}