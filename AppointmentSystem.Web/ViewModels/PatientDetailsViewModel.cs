using System.Security.Cryptography.Pkcs;

namespace MediBook.ViewModels
{
    public class PatientDetailsViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ProfilePicture { get; set; } = "default.png";
        public List<AppointmentDetailViewModel> Appointments { get; set; } = new();

    }

    public class AppointmentDetailViewModel
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string ClinicianNotes { get; set; } = string.Empty;



    }
}
