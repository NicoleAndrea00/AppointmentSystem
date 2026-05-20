using System.Text;
using System.Text.Json;

class Program
{
    static readonly HttpClient client = new HttpClient();
    static readonly string baseUrl = "https://localhost:72
        
        
        1/api";

    static async Task Main(string[] args)
    {
        Console.WriteLine("================================");
        Console.WriteLine("     MediBook Console Client    ");
        Console.WriteLine("================================");

        bool running = true;
        while (running)
        {
            Console.WriteLine("\nMain Menu:");
            Console.WriteLine("1. View all appointments");
            Console.WriteLine("2. View appointments by patient");
            Console.WriteLine("3. View appointments by clinician");
            Console.WriteLine("4. View all patients");
            Console.WriteLine("5. View all clinicians");
            Console.WriteLine("6. Create appointment");
            Console.WriteLine("7. Update appointment status");
            Console.WriteLine("8. Delete appointment");
            Console.WriteLine("0. Exit");
            Console.Write("\nEnter option: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    await GetAllAppointments();
                    break;
                case "2":
                    Console.Write("Enter Patient ID: ");
                    var patientId = Console.ReadLine();
                    await GetAppointmentsByPatient(patientId!);
                    break;
                case "3":
                    Console.Write("Enter Clinician ID: ");
                    var clinicianId = Console.ReadLine();
                    await GetAppointmentsByClinician(clinicianId!);
                    break;
                case "4":
                    await GetAllPatients();
                    break;
                case "5":
                    await GetAllClinicians();
                    break;
                case "6":
                    await CreateAppointment();
                    break;
                case "7":
                    await UpdateAppointment();
                    break;
                case "8":
                    Console.Write("Enter Appointment ID to delete: ");
                    var deleteId = Console.ReadLine();
                    await DeleteAppointment(deleteId!);
                    break;
                case "0":
                    running = false;
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again.");
                    break;
            }
        }
    }

    static async Task GetAllAppointments()
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/appointments");
            var json = await response.Content.ReadAsStringAsync();
            var appointments = JsonSerializer.Deserialize<JsonElement>(json);

            Console.WriteLine("\n--- All Appointments ---");
            foreach (var a in appointments.EnumerateArray())
            {
                Console.WriteLine($"ID: {a.GetProperty("id")} | " +
                    $"Date: {a.GetProperty("appointmentDate").GetString()?[..10]} | " +
                    $"Status: {a.GetProperty("status")} | " +
                    $"Patient: {a.GetProperty("patient").GetProperty("fullName")} | " +
                    $"Clinician: {a.GetProperty("clinician").GetProperty("fullName")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task GetAppointmentsByPatient(string patientId)
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/appointments/patient/{patientId}");
            var json = await response.Content.ReadAsStringAsync();
            var appointments = JsonSerializer.Deserialize<JsonElement>(json);

            Console.WriteLine($"\n--- Appointments for Patient {patientId} ---");
            foreach (var a in appointments.EnumerateArray())
            {
                Console.WriteLine($"ID: {a.GetProperty("id")} | " +
                    $"Date: {a.GetProperty("appointmentDate").GetString()?[..10]} | " +
                    $"Status: {a.GetProperty("status")} | " +
                    $"Clinician: {a.GetProperty("clinician").GetProperty("fullName")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task GetAppointmentsByClinician(string clinicianId)
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/appointments/clinician/{clinicianId}");
            var json = await response.Content.ReadAsStringAsync();
            var appointments = JsonSerializer.Deserialize<JsonElement>(json);

            Console.WriteLine($"\n--- Appointments for Clinician {clinicianId} ---");
            foreach (var a in appointments.EnumerateArray())
            {
                Console.WriteLine($"ID: {a.GetProperty("id")} | " +
                    $"Date: {a.GetProperty("appointmentDate").GetString()?[..10]} | " +
                    $"Status: {a.GetProperty("status")} | " +
                    $"Patient: {a.GetProperty("patient").GetProperty("fullName")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task GetAllPatients()
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/users/patients");
            var json = await response.Content.ReadAsStringAsync();
            var patients = JsonSerializer.Deserialize<JsonElement>(json);

            Console.WriteLine("\n--- All Patients ---");
            foreach (var p in patients.EnumerateArray())
            {
                Console.WriteLine($"ID: {p.GetProperty("id")} | " +
                    $"Name: {p.GetProperty("fullName")} | " +
                    $"Email: {p.GetProperty("email")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task GetAllClinicians()
    {
        try
        {
            var response = await client.GetAsync($"{baseUrl}/users/clinicians");
            var json = await response.Content.ReadAsStringAsync();
            var clinicians = JsonSerializer.Deserialize<JsonElement>(json);

            Console.WriteLine("\n--- All Clinicians ---");
            foreach (var c in clinicians.EnumerateArray())
            {
                Console.WriteLine($"ID: {c.GetProperty("id")} | " +
                    $"Name: {c.GetProperty("fullName")} | " +
                    $"Speciality: {c.GetProperty("speciality")}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task CreateAppointment()
    {
        try
        {
            Console.Write("Enter Patient ID: ");
            var patientId = int.Parse(Console.ReadLine()!);

            Console.Write("Enter Clinician ID: ");
            var clinicianId = int.Parse(Console.ReadLine()!);

            Console.Write("Enter Date (yyyy-MM-dd HH:mm): ");
            var date = DateTime.Parse(Console.ReadLine()!);

            Console.Write("Enter Notes (optional): ");
            var notes = Console.ReadLine();

            var appointment = new
            {
                patientId,
                clinicianId,
                appointmentDate = date,
                notes = notes ?? "",
                status = "Scheduled"
            };

            var json = JsonSerializer.Serialize(appointment);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/appointments", content);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ Appointment created successfully!");
            else
                Console.WriteLine($"❌ Failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task UpdateAppointment()
    {
        try
        {
            Console.Write("Enter Appointment ID to update: ");
            var id = int.Parse(Console.ReadLine()!);

            Console.Write("Enter new status (Scheduled/Confirmed/Cancelled/Completed): ");
            var status = Console.ReadLine();

            Console.Write("Enter new date (yyyy-MM-dd HH:mm) or press Enter to skip: ");
            var dateInput = Console.ReadLine();
            var date = string.IsNullOrEmpty(dateInput) ? DateTime.Now : DateTime.Parse(dateInput);

            var update = new
            {
                appointmentDate = date,
                status = status ?? "Scheduled",
                notes = ""
            };

            var json = JsonSerializer.Serialize(update);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{baseUrl}/appointments/{id}", content);

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ Appointment updated successfully!");
            else
                Console.WriteLine($"❌ Failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static async Task DeleteAppointment(string id)
    {
        try
        {
            Console.Write($"Are you sure you want to delete appointment {id}? (y/n): ");
            var confirm = Console.ReadLine();

            if (confirm?.ToLower() != "y")
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            var response = await client.DeleteAsync($"{baseUrl}/appointments/{id}");

            if (response.IsSuccessStatusCode)
                Console.WriteLine("✅ Appointment deleted successfully!");
            else
                Console.WriteLine($"❌ Failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}