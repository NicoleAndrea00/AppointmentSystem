using Microsoft.AspNetCore.Mvc;
using MediBook.Data;
using MediBook.Models;
using MediBook.ViewModels;
using BCrypt.Net;

namespace MediBook.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppointmentDbContext _context;

        public AccountController(AppointmentDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "An account with this email already exists");
                return View(model);
            }

            string profilePictureName = "defaultimg.png";
            if(model.ProfilePicture !=null && model.ProfilePicture.Length > 0)
            {
                // Only allow image files
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ProfilePicture", "Only image files are allowed (jpg, jpeg, png, gif)");
                    return View(model);
                }

                // Generate unique filename
                profilePictureName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                var filePath = Path.Combine(uploadsFolder, profilePictureName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // If registering as a Clinician, create their profile
            if (model.Role == "Clinician")
            {
                var clinician = new Clinician
                {
                    UserId = user.Id,
                    Speciality = "General",
                    Department = "General"
                };
                _context.Clinicians.Add(clinician);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginviewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("ProfilePicture", user.ProfilePicture ?? "default.png");

            // Redirect based on role
            return user.Role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Clinician" => RedirectToAction("Index", "Clinician"),
                _ => RedirectToAction("Index", "Patient")
            };
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}