using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CurrentProfilePicture { get; set; }

        public IFormFile? ProfilePicture { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string? ConfirmNewPassword { get; set; }
    }
}