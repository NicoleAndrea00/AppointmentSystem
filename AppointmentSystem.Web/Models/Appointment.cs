namespace AppointmentSystem.Web.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Scheduled";
        public string Notes { get; set; } = string.Empty;
        public string ClinicalNotes {  get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        //Foreign Keys
        public int PatientId { get; set; }
        public User Patient {  get; set; }
        
        public int ClinicianId { get; set; }
        public Clinician Clinician { get; set; } = null!;

    }
}
