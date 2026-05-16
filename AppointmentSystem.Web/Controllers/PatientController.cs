using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediBook.Controllers
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
            ViewBag.ProfilePicture = HttpContext.Session.GetString("ProfilePicture"); 

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
                ConsultationType = model.ConsultationType,
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
        // GET: /Patient/Profile
        public async Task<IActionResult> Profile()
        {
            if (!IsPatient())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                CurrentProfilePicture = user.ProfilePicture,
                InsuranceProvider = user.InsuranceProvider,
                InsuranceMemberNumber = user.InsuranceMemberNumber
            };

            return View(model);
        }

        // POST: /Patient/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!IsPatient())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            
            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProfilePicture", "Only image files are allowed");
                    model.CurrentProfilePicture = user.ProfilePicture;
                    return View(model);
                }

                
                if (user.ProfilePicture != "default.png")
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", user.ProfilePicture);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                
                var newFileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicture = newFileName;
                HttpContext.Session.SetString("ProfilePicture", newFileName);
            }

            
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (!ModelState.IsValid)
                {
                    model.CurrentProfilePicture = user.ProfilePicture;
                    return View(model);
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }
            user.InsuranceProvider = model.InsuranceProvider;
            user.InsuranceMemberNumber = model.InsuranceMemberNumber;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
    }
}