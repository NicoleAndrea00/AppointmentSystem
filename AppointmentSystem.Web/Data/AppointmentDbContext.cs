using Microsoft.EntityFrameworkCore;
using MediBook.Models;

namespace MediBook.Data
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
            : base(options)
        {
        }
        public DbSet<User>Users { get; set; }
        public DbSet<Clinician>Clinicians { get; set; }
        public DbSet<Appointment>Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //User => Patient Appointments
            modelBuilder.Entity<Appointment>() 
                .HasOne(p => p.Patient)
                .WithMany(a => a.PatientAppointments)
                .HasForeignKey(i =>i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            //Clinician => Appointments
            modelBuilder.Entity<Appointment>()
                .HasOne(c => c.Clinician)
                .WithMany(a => a.Appointments)
                .HasForeignKey(i => i.ClinicianId)
                .OnDelete(DeleteBehavior.Restrict);

            //User => clinicanProfile
            modelBuilder.Entity<Clinician>()
                .HasOne(c => c.User)
                .WithOne(u => u.ClinicianProfile)
                .HasForeignKey<Clinician>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
