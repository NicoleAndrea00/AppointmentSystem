using AppointmentSystem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppointmentDbContext _context;
        public PatientController(AppointmentDbContext context)
        {
            _context = context;
        }

        private bool IsPatient()
        {
            return HttpContext.Session.GetString("UserRole") == "Patient";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsPatient())
                return RedirectToAction("Login", "Account");
            var userId = HttpContext.Session.GetInt32("UserId");

            var appointments = await _context.Appointments
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .Where (a => a.PatientId == userId)
                .OrderBy (a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(appointments);

        }
    }
}
