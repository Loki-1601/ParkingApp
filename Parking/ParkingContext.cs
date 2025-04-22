
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Represents the Entity Framework Core database context for the Parking application.
/// This class is responsible for managing the database connection, configuring the database provider,
/// and defining the relationships between entities in the Parking application.
/// </summary>
/// <remarks>
/// Key Points for Junior Developers:
/// - <see cref="ParkingContext"/> inherits from <see cref="DbContext"/>, which is the base class for EF Core.
/// - The <see cref="DbSet{TEntity}"/> properties (e.g., <see cref="Permits"/>, <see cref="ParkingReservations"/>, <see cref="ParkingAssignments"/>)
///   represent the tables in the database.
/// - The <see cref="OnConfiguring"/> method is used to configure the database connection string and logging behavior.
/// - The <see cref="OnModelCreating"/> method is used to define relationships and constraints between entities.
/// - Logging is enabled using a custom <see cref="ILoggerFactory"/> to help debug database queries.
/// - Sensitive data logging is enabled for development purposes but should be disabled in production for security reasons.
/// </remarks>
/// <example>
/// Example usage:
/// <code>
/// var options = new DbContextOptionsBuilder<ParkingContext>()
///     .UseMySQL("server=localhost;database=dev_db;user=root;password=yourpassword")
///     .Options;
/// using var context = new ParkingContext(options);
/// var permits = context.Permits.ToList();
/// </code>
/// </example>
/// 
/// Relationships between entities:
/// - A `ParkingAssignment` is linked to a `Permit`:
///   - Each `ParkingAssignment` must have one `Permit` (via `PermitId`).
///   - A `Permit` can have multiple `ParkingAssignments`.
/// - A `ParkingAssignment` is also linked to a `ParkingReservation`:
///   - Each `ParkingAssignment` can optionally have one `ParkingReservation`.
/// - `ParkingReservation` and `Permit` are independent entities with their own primary keys.
///
/// Visual representation of relationships:
/// ```
/// Permit (1) -------------------< (0..*) ParkingAssignment >------------------- (0..1) ParkingReservation
/// ```
/// Legend:
/// - (1): One
/// - (0..*): Zero or many
/// - (0..1): Zero or one

namespace ParkingApp.Parking
{
    public class ParkingContext : DbContext
    {
        // defined entities (tables) of our database/system
        public DbSet<Permit> Permits { get; set; }
        public DbSet<ParkingReservation> ParkingReservations { get; set; }
        public DbSet<ParkingAssignment> ParkingAssignments { get; set; }

        private static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); // Logs to the console
        });

        // Constructor that takes DbContextOptions
        public ParkingContext(DbContextOptions<ParkingContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySQL("server=localhost;database=dev_db;user=root;password=Kamala@16")
                .UseLoggerFactory(loggerFactory) // Add the logger factory
                .EnableSensitiveDataLogging(); // Optional: logs parameter values for queries
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // configure links or relationships between entities

            // 1. Permit to ParkingAssignment
            // 1-to-many relationship
            // A Permit can have multiple ParkingAssignments
            // A ParkingAssignment must have one Permit
            modelBuilder.Entity<ParkingAssignment>(builder =>
            {
                builder.HasKey(e => e.Id);
                builder.HasOne(p => p.Permit)
                    .WithMany(p => p.ParkingAssignments)
                    .HasForeignKey(p => p.PermitId);

                builder.HasOne(p => p.ParkingReservation);
            });

            modelBuilder.Entity<ParkingReservation>().HasKey(e => e.Id);
            modelBuilder.Entity<Permit>().HasKey(e => e.Id);
        }
    }
}
