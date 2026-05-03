using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MediBook.ViewModels
{
    public class BookAppointment
    {
        [Required]
        public int ClinicianId { get; set;  }
        [Required]
        public DateTime AppointmentDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Clinicians { get; set; } = new List<SelectListItem>();


    }
}
