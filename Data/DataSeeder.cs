using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Data
{
    /// <summary>
    /// Classe d'initialisation de données (Seed) pour peupler la base de données au démarrage.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(TransportRimDbContext context)
        {
            // S'assurer que la base de données est créée et à jour via les migrations
            await context.Database.MigrateAsync();

            // 1. Création de l'Administrateur par défaut s'il n'existe pas
            if (!await context.Users.AnyAsync(u => u.Email == "admin@transport.mr"))
            {
                var admin = new User
                {
                    Name = "Administrateur Système",
                    Email = "admin@transport.mr",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(admin);
            }

            // 2. Création d'un Voyageur de test s'il n'existe pas
            if (!await context.Users.AnyAsync(u => u.Email == "traveler@gmail.com"))
            {
                var traveler = new User
                {
                    Name = "Cheikh Traveler",
                    Email = "traveler@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TravelerPassword123!"),
                    Role = UserRole.Traveler,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(traveler);
            }

            await context.SaveChangesAsync();

            // 3. Création des compagnies si la table est vide
            if (!await context.Companies.AnyAsync())
            {
                var sogetrog = new Company
                {
                    Name = "Sogetrog Express",
                    Phone = "36112233",
                    Email = "sogetrog@gmail.com",
                    Address = "Gare Sogetrog, Madrid, Nouakchott",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                var moussafir = new Company
                {
                    Name = "El Moussafir Express",
                    Phone = "46445566",
                    Email = "moussafir@gmail.com",
                    Address = "Carrefour Madrid, Nouakchott",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                context.Companies.AddRange(sogetrog, moussafir);
                await context.SaveChangesAsync();

                // 4. Création des utilisateurs "Gestionnaire Compagnie" associés
                var sogetrogUser = new User
                {
                    Name = "Gestionnaire Sogetrog",
                    Email = "sogetrog@company.mr",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("CompanyPassword123!"),
                    Role = UserRole.Company,
                    CompanyId = sogetrog.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var moussafirUser = new User
                {
                    Name = "Gestionnaire Moussafir",
                    Email = "moussafir@company.mr",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("CompanyPassword123!"),
                    Role = UserRole.Company,
                    CompanyId = moussafir.Id,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.AddRange(sogetrogUser, moussafirUser);
                await context.SaveChangesAsync();

                // 5. Création des Bus associés
                var bus1 = new Bus
                {
                    CompanyId = sogetrog.Id,
                    BusNumber = "1234AA00",
                    Capacity = 50
                };

                var bus2 = new Bus
                {
                    CompanyId = sogetrog.Id,
                    BusNumber = "5678AA00",
                    Capacity = 30
                };

                var bus3 = new Bus
                {
                    CompanyId = moussafir.Id,
                    BusNumber = "9999AA00",
                    Capacity = 45
                };

                context.Buses.AddRange(bus1, bus2, bus3);
                await context.SaveChangesAsync();

                // 6. Création des Trajets
                var trip1 = new Trip
                {
                    BusId = bus1.Id,
                    DepartureCity = "Nouakchott",
                    ArrivalCity = "Nouadhibou",
                    Date = DateTime.UtcNow.AddDays(2).Date,
                    Time = new TimeSpan(8, 0, 0),
                    Price = 700.00m,
                    AvailableSeats = bus1.Capacity
                };

                var trip2 = new Trip
                {
                    BusId = bus2.Id,
                    DepartureCity = "Nouakchott",
                    ArrivalCity = "Rosso",
                    Date = DateTime.UtcNow.AddDays(1).Date,
                    Time = new TimeSpan(14, 30, 0),
                    Price = 300.00m,
                    AvailableSeats = bus2.Capacity
                };

                var trip3 = new Trip
                {
                    BusId = bus3.Id,
                    DepartureCity = "Nouadhibou",
                    ArrivalCity = "Nouakchott",
                    Date = DateTime.UtcNow.AddDays(3).Date,
                    Time = new TimeSpan(9, 0, 0),
                    Price = 700.00m,
                    AvailableSeats = bus3.Capacity
                };

                context.Trips.AddRange(trip1, trip2, trip3);
                await context.SaveChangesAsync();
            }
        }
    }
}
