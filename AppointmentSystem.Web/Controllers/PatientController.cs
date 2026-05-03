using AppointmentSystem.Web.Data;
using AppointmentSystem.Web.Models;
using AppointmentSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                .Where(a => a.PatientId == userId)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(appointments);
        }

        // GET: /Patient/Book
        public async Task<IActionResult> Book()
        {
            if (!IsPatient())
                return RedirectToAction("Login", "Account");

            var clinicians = await _context.Clinicians
                .Include(c => c.User)
                .ToListAsync();

            var model = new BookAppointment
            {
                Clinicians = clinicians.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"Dr. {c.User.FullName} - {c.Speciality}"
                })
            };

            return View(model);
        }

        // POST: /Patient/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointment model)
        {
            if (!IsPatient())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                var clinicians = await _context.Clinicians
                    .Include(c => c.User)
                    .ToListAsync();

                model.Clinicians = clinicians.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"Dr. {c.User.FullName} - {c.Speciality}"
                });

                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            var appointment = new Appointment
            {
                PatientId = userId!.Value,
                ClinicianId = model.ClinicianId,
                AppointmentDate = model.AppointmentDate,
                Notes = model.Notes,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Patient/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            if (!IsPatient())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.PatientId == userId);

            if (appointment == null)
                return RedirectToAction("Index");

            appointment.Status = "Cancelled";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment cancelled successfully!";
            return RedirectToAction("Index");
        }
    }
}