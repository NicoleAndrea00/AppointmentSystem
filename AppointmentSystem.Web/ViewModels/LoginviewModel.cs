using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class LoginviewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
