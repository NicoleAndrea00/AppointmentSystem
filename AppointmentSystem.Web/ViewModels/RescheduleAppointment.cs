using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class RescheduleAppointmentViewModel
    {
        public int AppointmentId { get; set; }
        public string ClinicianName { get; set; } = string.Empty;
        public DateTime CurrentDate { get; set; }

        [Required]
        [CustomValidation(typeof(RescheduleAppointmentViewModel), "ValidateFutureDate")]
        public DateTime NewAppointmentDate { get; set; } = DateTime.Now;

        public static ValidationResult ValidateFutureDate(DateTime date, ValidationContext context)
        {
            if (date <= DateTime.Now)
                return new ValidationResult("Appointment date must be in the future");
            return ValidationResult.Success!;
        }
    }
}