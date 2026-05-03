namespace MediBook.Models
{
    public class Clinician
    {
        public int Id { get; set; }
        public string Speciality { get; set; } = string.Empty;
        public string Department {  get; set; } = string.Empty;

        //FK back to user
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<Appointment>Appointments { get; set; } = new List<Appointment>();
    }
}
