using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppointmentDbContext _context;

        public AdminController(AppointmentDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.TotalAppointments = appointments.Count;
            ViewBag.ScheduledCount = appointments.Count(a => a.Status == "Scheduled");
            ViewBag.ConfirmedCount = appointments.Count(a => a.Status == "Confirmed");
            ViewBag.CancelledCount = appointments.Count(a => a.Status == "Cancelled");
            ViewBag.CompletedCount = appointments.Count(a => a.Status == "Completed");
            ViewBag.FaceToFaceCount = appointments.Count(a => a.ConsultationType == "Face to Face");
            ViewBag.VideoCallCount = appointments.Count(a => a.ConsultationType == "Video Call");

            return View(appointments);
        }
        // GET: /Admin/Calendar
        public async Task<IActionResult> Calendar()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View();
        }

        // GET: /Admin/CalendarEvents
        public async Task<IActionResult> CalendarEvents()
        {
            if (!IsAdmin())
                return Unauthorized();

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .ToListAsync();

            var events = appointments.Select(a => new
            {
                id = a.Id,
                title = $"{a.Patient.FullName} - Dr. {a.Clinician.User.FullName}",
                start = a.AppointmentDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                end = a.AppointmentDate.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss"),
                color = a.Status == "Confirmed" ? "#198754" :
                        a.Status == "Cancelled" ? "#dc3545" :
                        a.Status == "Completed" ? "#6c757d" : "#0d6efd",
                extendedProps = new { status = a.Status, consultationType = a.ConsultationType }
            });

            return Json(events);
        }

        // GET: /Admin/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            await PopulateDropdowns();
            return View();
        }

        // POST: /Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns();
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            model.Status = "Scheduled";
            _context.Appointments.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment created successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return RedirectToAction("Index");

            await PopulateDropdowns();
            return View(appointment);
        }

        // POST: /Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return RedirectToAction("Index");

            appointment.PatientId = model.PatientId;
            appointment.ClinicianId = model.ClinicianId;
            appointment.AppointmentDate = model.AppointmentDate;
            appointment.Status = model.Status;
            appointment.Notes = model.Notes;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment updated successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinician)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return RedirectToAction("Index");

            return View(appointment);
        }

        // POST: /Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Appointment deleted successfully!";
            return RedirectToAction("Index");
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var users = await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // Helper to populate dropdowns
        private async Task PopulateDropdowns()
        {
            var patients = await _context.Users
                .Where(u => u.Role == "Patient")
                .ToListAsync();

            var clinicians = await _context.Clinicians
                .Include(c => c.User)
                .ToListAsync();

            ViewBag.Patients = patients.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FullName
            });

            ViewBag.Clinicians = clinicians.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"Dr. {c.User.FullName} - {c.Speciality}"
            });

            ViewBag.Statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Scheduled", Text = "Scheduled" },
                new SelectListItem { Value = "Confirmed", Text = "Confirmed" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" },
                new SelectListItem { Value = "Completed", Text = "Completed" }
            };
        }
    }
}