namespace MediBook.Models
{
    public class User
    {
        public int Id { get; set; }
        public string ProfilePicture { get; set; } = "default.png";
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string InsuranceProvider { get; set; }
        public string InsuranceMemberNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;  //Patient, Clinician, Admin
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();
        public Clinician ClinicianProfile { get; set; }

    }
}
