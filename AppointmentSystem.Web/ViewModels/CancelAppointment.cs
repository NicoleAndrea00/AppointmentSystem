using Microsoft.Playwright;
using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class CancelAppointment
    {
        public int AppointmentId { get; set; }
        public string ClinicianName { get; set; }
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Please select a reason for cancellation")]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
