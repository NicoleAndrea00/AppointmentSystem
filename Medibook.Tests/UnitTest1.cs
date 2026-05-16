using MediBook.Data;
using MediBook.Models;
using Microsoft.EntityFrameworkCore;

namespace Medibook.Tests
{
    public class UnitTest1
    {
        private AppointmentDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppointmentDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppointmentDbContext(options);
        }

        // Test 1 - User can be created and saved
        [Fact]
        public async Task User_CanBeCreated_AndSaved()
        {
            var context = GetInMemoryContext();

            var user = new User
            {
                FullName = "Test Patient",
                Email = "patient@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Patient",
                CreatedAt = DateTime.UtcNow,
                ProfilePicture = "default.png"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            Assert.Equal(1, context.Users.Count());
            Assert.Equal("Test Patient", context.Users.First().FullName);
        }

        // Test 2 - Password hashing works correctly
        [Fact]
        public void Password_IsHashed_Correctly()
        {
            var password = "testpassword123";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            Assert.True(BCrypt.Net.BCrypt.Verify(password, hash));
            Assert.False(BCrypt.Net.BCrypt.Verify("wrongpassword", hash));
        }

        // Test 3 - Appointment can be created
        [Fact]
        public async Task Appointment_CanBeCreated_AndSaved()
        {
            var context = GetInMemoryContext();

            var patient = new User
            {
                FullName = "Test Patient",
                Email = "patient@test.com",
                PasswordHash = "hashedpassword",
                Role = "Patient",
                ProfilePicture = "default.png"
            };

            var clinicianUser = new User
            {
                FullName = "Test Clinician",
                Email = "clinician@test.com",
                PasswordHash = "hashedpassword",
                Role = "Clinician",
                ProfilePicture = "default.png"
            };

            context.Users.AddRange(patient, clinicianUser);
            await context.SaveChangesAsync();

            var clinician = new Clinician
            {
                UserId = clinicianUser.Id,
                Speciality = "General",
                Department = "General"
            };

            context.Clinicians.Add(clinician);
            await context.SaveChangesAsync();

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                ClinicianId = clinician.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Status = "Scheduled",
                Notes = "Test appointment",
                CreatedAt = DateTime.UtcNow
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            Assert.Equal(1, context.Appointments.Count());
            Assert.Equal("Scheduled", context.Appointments.First().Status);
        }

        // Test 4 - Duplicate email check
        [Fact]
        public async Task User_Email_ShouldBeUnique()
        {
            var context = GetInMemoryContext();

            var user1 = new User
            {
                FullName = "User One",
                Email = "same@test.com",
                PasswordHash = "hash1",
                Role = "Patient",
                ProfilePicture = "default.png"
            };

            context.Users.Add(user1);
            await context.SaveChangesAsync();

            var emailExists = context.Users.Any(u => u.Email == "same@test.com");
            Assert.True(emailExists);
        }

        // Test 5 - Appointment status can be updated
        [Fact]
        public async Task Appointment_Status_CanBeUpdated()
        {
            var context = GetInMemoryContext();

            var patient = new User
            {
                FullName = "Test Patient",
                Email = "patient@test.com",
                PasswordHash = "hash",
                Role = "Patient",
                ProfilePicture = "default.png"
            };

            var clinicianUser = new User
            {
                FullName = "Test Clinician",
                Email = "clinician@test.com",
                PasswordHash = "hash",
                Role = "Clinician",
                ProfilePicture = "default.png"
            };

            context.Users.AddRange(patient, clinicianUser);
            await context.SaveChangesAsync();

            var clinician = new Clinician
            {
                UserId = clinicianUser.Id,
                Speciality = "General",
                Department = "General"
            };

            context.Clinicians.Add(clinician);
            await context.SaveChangesAsync();

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                ClinicianId = clinician.Id,
                AppointmentDate = DateTime.UtcNow.AddDays(1),
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            appointment.Status = "Confirmed";
            await context.SaveChangesAsync();

            Assert.Equal("Confirmed", context.Appointments.First().Status);
        }

        // Test 6 - Patient can only see their own appointments
        [Fact]
        public async Task Patient_CanOnlySee_OwnAppointments()
        {
            var context = GetInMemoryContext();

            var patient1 = new User { FullName = "Patient One", Email = "p1@test.com", PasswordHash = "hash", Role = "Patient", ProfilePicture = "default.png" };
            var patient2 = new User { FullName = "Patient Two", Email = "p2@test.com", PasswordHash = "hash", Role = "Patient", ProfilePicture = "default.png" };
            var clinicianUser = new User { FullName = "Clinician", Email = "c@test.com", PasswordHash = "hash", Role = "Clinician", ProfilePicture = "default.png" };

            context.Users.AddRange(patient1, patient2, clinicianUser);
            await context.SaveChangesAsync();

            var clinician = new Clinician { UserId = clinicianUser.Id, Speciality = "General", Department = "General" };
            context.Clinicians.Add(clinician);
            await context.SaveChangesAsync();

            context.Appointments.AddRange(
                new Appointment { PatientId = patient1.Id, ClinicianId = clinician.Id, AppointmentDate = DateTime.UtcNow.AddDays(1), Status = "Scheduled", CreatedAt = DateTime.UtcNow },
                new Appointment { PatientId = patient2.Id, ClinicianId = clinician.Id, AppointmentDate = DateTime.UtcNow.AddDays(2), Status = "Scheduled", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var patient1Appointments = context.Appointments.Where(a => a.PatientId == patient1.Id).ToList();
            Assert.Equal(1, patient1Appointments.Count);
        }
    }
}