using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class BookAppointment
    {
        [Required]
        public int ClinicianId { get; set;  }
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(BookAppointment), "ValidateFutureDate")]

        public DateTime AppointmentDate { get; set; } = DateTime.Now;
        public static ValidationResult ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date <= DateTime.Now)
                return new ValidationResult("Appointment cannot be set in the past");
            return ValidationResult.Success!;

        }

        [Required]
        public string ConsultationType { get; set; } = "Face to Face";
        public string Notes { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Clinicians { get; set; } = new List<SelectListItem>();


    }
}
