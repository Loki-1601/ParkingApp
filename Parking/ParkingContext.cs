using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ParkingApp.Parking
{
    public class ParkingContext : DbContext
    {
        public DbSet<Permit>? Permits { get; set; }
        public DbSet<ParkingReservation>? ParkingReservations { get; set; }
        public DbSet<ParkingAssignment>? ParkingAssignments { get; set; }
        // public DbSet<Permit>? Permit { get; set; }
        // public DbSet<ParkingReservation>? ParkingReservation { get; set; }
        // public DbSet<ParkingAssignment>? ParkingAssignment { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=localhost;database=dev_db;user=root;password=Kamala@16");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParkingAssignment>()
                .HasOne(p => p.Permit)
                .WithMany(p => p.ParkingAssignments)
                .HasForeignKey(p => p.PermitId);
        }

    }
}
