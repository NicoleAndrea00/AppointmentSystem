using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class RegisterViewModel
    {
        [Required (ErrorMessage = "Please enter Full Name")]
        public string FullName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Please Enter Email")]
        [EmailAddress(ErrorMessage ="Please Enter Valid Email Address")]
        public string Email {  get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password {  get; set; } = string.Empty;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not Match")]
        public string ConfirmPassword {  get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
        public IFormFile? ProfilePicture { get; set; }

    }
}
