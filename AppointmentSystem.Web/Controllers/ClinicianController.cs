using AppointmentSystem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Web.Controllers
{
    public class ClinicianController : Controller
    {
        private readonly AppointmentDbContext _context;
        public ClinicianController(AppointmentDbContext context)
        {
            _context = context;
        }
        private bool IsClinician()
        {
            return HttpContext.Session.GetString("UserRole") == "Clinician";
        }
        public async Task<IActionResult> Index()
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (clinician == null)
                return RedirectToAction("Login", "Account");

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.ClinicianId == clinician.Id)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.Specialty = clinician.Speciality;

            return View(appointments);
        }
        public async Task<IActionResult> Confirm(int id)
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var clinician = await _context.Clinicians .FirstOrDefaultAsync(a => a.UserId == userId);
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.ClinicianId == clinician!.Id);

            if (appointment == null)
                return RedirectToAction("Index");
            appointment.Status = "success";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Appointment confirmed";
            return RedirectToAction("Index");

        }
    }
}
