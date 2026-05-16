using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediBook.Data;

namespace MediBook.ApiControllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersApiController : ControllerBase
    {
        private readonly AppointmentDbContext _context;

        public UsersApiController(AppointmentDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // GET: api/users/patients
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Users
                .Where(u => u.Role == "Patient")
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(patients);
        }

        // GET: api/users/clinicians
        [HttpGet("clinicians")]
        public async Task<IActionResult> GetClinicians()
        {
            var clinicians = await _context.Clinicians
                .Include(c => c.User)
                .Select(c => new
                {
                    c.Id,
                    c.User.FullName,
                    c.User.Email,
                    c.Speciality,
                    c.Department
                })
                .ToListAsync();

            return Ok(clinicians);
        }
    }
}