using System.ComponentModel.DataAnnotations;

namespace AppointmentSystem.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email {  get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        public string Password {  get; set; } = string.Empty;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not Match")]
        public string ConfirmPassword {  get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;

    }
}
