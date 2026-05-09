using MediBook.Data;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers
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

        // GET: /Clinician/PatientDetails/5
        public async Task<IActionResult> PatientDetails(int id)
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId);

            //patient has appointment with this clinician
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClinicianId == clinician!.Id);

            if (appointment == null)
                return RedirectToAction("Index");

            // Gets all appointments this patient has with this clinician
            var allAppointments = await _context.Appointments
                .Where(a => a.PatientId == appointment.PatientId && a.ClinicianId == clinician!.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var model = new PatientDetailsViewModel
            {
                FullName = appointment.Patient.FullName,
                Email = appointment.Patient.Email,
                ProfilePicture = appointment.Patient.ProfilePicture ?? "default.png",
                Appointments = allAppointments.Select(a => new AppointmentDetailViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes,
                    ClinicianNotes = a.ClinicalNotes
                }).ToList()
            };

            return View(model);
        }

        
        public async Task<IActionResult> AddNotes(int id)
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClinicianId == clinician!.Id);

            if (appointment == null)
                return RedirectToAction("Index");

            ViewBag.AppointmentId = id;
            ViewBag.PatientName = appointment.Patient.FullName;
            ViewBag.CurrentNotes = appointment.ClinicalNotes;

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNotes(int id, string clinicalNotes)
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.ClinicianId == clinician!.Id);

            if (appointment == null)
                return RedirectToAction("Index");

            appointment.ClinicalNotes = clinicalNotes;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Clinical notes updated!";
            return RedirectToAction("Index");
        }

   
        public async Task<IActionResult> Reschedule(int id)
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id && a.ClinicianId == clinician!.Id);

            if (appointment == null)
                return RedirectToAction("Index");

            ViewBag.AppointmentId = id;
            ViewBag.PatientName = appointment.Patient.FullName;
            ViewBag.CurrentDate = appointment.AppointmentDate.ToString("yyyy-MM-ddTHH:mm");

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime appointmentDate)
        {
            if (!IsClinician())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var clinician = await _context.Clinicians
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.ClinicianId == clinician!.Id);

            if (appointment == null)
                return RedirectToAction("Index");

            appointment.AppointmentDate = appointmentDate;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Appointment rescheduled!";
            return RedirectToAction("Index");
        }
    }
}
