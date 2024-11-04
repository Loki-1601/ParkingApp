using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ParkingApp.Parking.Tests
{
    public class UtilityUnitTests
    {
        private ParkingContext GetInMemoryDbContext()
        {
            
            var options = new DbContextOptionsBuilder<ParkingContext>()
                // .UseInMemoryDatabase(databaseName: "TestDatabase")
                .UseMySQL("server=localhost;database=dev_db;user=root;password=Kamala@16")
                .Options;

            return new ParkingContext(options);
        }

        [Fact]
        public void ResetDb_ShouldDropAndRecreateTables()
        {
            // Arrange, Act, Assert

            // Arrange
            Utility.resetDb();
            var dbContext = GetInMemoryDbContext();

            // Act
            var tablesExist = dbContext.ParkingReservations.Any();

            // Assert
            Assert.False(tablesExist, "Tables should be reset and empty.");
        }

        [Fact]

        public void DropParkingAssignments_ShouldRemoveAllAssignments()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            dbContext.ParkingAssignments.Add(new ParkingAssignment { PermitId = 1, ParkingReservationId = 1 });
            dbContext.SaveChanges();

            // Act
            Utility.dropParkingAssignments();

            // Assert
            Assert.Empty(dbContext.ParkingAssignments.ToList());
        }


        [Fact]
        public void SeedParkingReservations_ShouldInsertReservations()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();

            // Act
            Utility.seedParkingReservations();

            // Assert
            Assert.True(dbContext.ParkingReservations.Count() > 0, "Should have seeded reservations.");
        }

    }
}
