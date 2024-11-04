using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ParkingApp.Parking
{
    public class ParkingContext : DbContext
    {
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
