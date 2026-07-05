using Microsoft.EntityFrameworkCore;
using TransportRim.Api.Entities;

namespace TransportRim.Api.Data
{
    /// <summary>
    /// Contexte de la base de données Entity Framework Core pour la plateforme Transport Rim.
    /// </summary>
    public class TransportRimDbContext : DbContext
    {
        public TransportRimDbContext(DbContextOptions<TransportRimDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Bus> Buses => Set<Bus>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuration de l'entité User ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(u => u.PhoneNumber).IsUnique(); // Index unique sur le téléphone
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Role)
                    .HasConversion<string>() // Conversion Enum -> String pour stockage propre
                    .HasMaxLength(20);

                entity.HasOne(u => u.Company)
                    .WithMany()
                    .HasForeignKey(u => u.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict); // Interdire la suppression d'une compagnie si elle a des utilisateurs

                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // --- Configuration de l'entité Company ---
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Address).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            });

            // --- Configuration de l'entité Bus ---
            modelBuilder.Entity<Bus>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.BusNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(b => b.BusNumber).IsUnique(); // Rend le numéro de bus globalement unique
                entity.Property(b => b.Capacity).IsRequired();
                
                // Relation One-To-Many (Une compagnie a plusieurs bus)
                entity.HasOne(b => b.Company)
                    .WithMany(c => c.Buses)
                    .HasForeignKey(b => b.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade); // Si on supprime une compagnie, ses bus sont supprimés
            });

            // --- Configuration de l'entité Trip ---
            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.DepartureCity).IsRequired().HasMaxLength(50);
                entity.Property(t => t.ArrivalCity).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Date).IsRequired();
                entity.Property(t => t.Time).IsRequired();
                entity.Property(t => t.Price).HasPrecision(18, 2).IsRequired();
                entity.Property(t => t.AvailableSeats).IsRequired();

                // Relation One-To-Many (Un bus effectue plusieurs trajets)
                entity.HasOne(t => t.Bus)
                    .WithMany(b => b.Trips)
                    .HasForeignKey(t => t.BusId)
                    .OnDelete(DeleteBehavior.Restrict); // Interdire la suppression d'un bus s'il a des trajets associés
            });

            // --- Configuration de l'entité Reservation ---
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.ReservedSeats).IsRequired();
                entity.Property(r => r.TotalPrice).HasPrecision(18, 2).IsRequired();
                entity.Property(r => r.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .HasDefaultValue(ReservationStatus.Pending);
                entity.Property(r => r.CreatedAt).IsRequired();

                // Relation One-To-Many (Un utilisateur fait plusieurs réservations)
                entity.HasOne(r => r.User)
                    .WithMany(u => u.Reservations)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Ne pas supprimer un utilisateur ayant des réservations actives

                // Relation One-To-Many (Un trajet a plusieurs réservations)
                entity.HasOne(r => r.Trip)
                    .WithMany(t => t.Reservations)
                    .HasForeignKey(r => r.TripId)
                    .OnDelete(DeleteBehavior.Restrict); // Ne pas supprimer un trajet réservé
            });

            // --- Configuration de l'entité Payment ---
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(p => p.Method)
                    .HasConversion<string>()
                    .HasMaxLength(20);
                entity.Property(p => p.TransactionId).IsRequired().HasMaxLength(100);
                entity.HasIndex(p => p.TransactionId).IsUnique(); // Index unique pour la transaction Masrivi/Bankily
                entity.Property(p => p.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");

                // Relation One-To-One (Une réservation a un paiement unique)
                entity.HasOne(p => p.Reservation)
                    .WithOne(r => r.Payment)
                    .HasForeignKey<Payment>(p => p.ReservationId)
                    .OnDelete(DeleteBehavior.Restrict); // Conserver le lien de paiement
            });

            // --- Configuration de l'entité Ticket ---
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(tk => tk.Id);
                entity.Property(tk => tk.QrCodeData).IsRequired().HasMaxLength(500);

                // Relation One-To-One (Une réservation a un ticket unique)
                entity.HasOne(tk => tk.Reservation)
                    .WithOne(r => r.Ticket)
                    .HasForeignKey<Ticket>(tk => tk.ReservationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- Configuration de l'entité Notification ---
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Type).IsRequired().HasMaxLength(20);
                entity.Property(n => n.Recipient).IsRequired().HasMaxLength(100);
                entity.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                entity.Property(n => n.SentAt).IsRequired();

                // Relation One-To-Many (Un utilisateur a plusieurs notifications)
                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
