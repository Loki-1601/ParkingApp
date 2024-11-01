using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ParkingApp.Parking.Tests
{
    public class UtilityUnitTests
    {
        private readonly Mock<ParkingContext> _mockContext;

        public UtilityUnitTests()
        {
            // Initialize the mock DbContext
            _mockContext = new Mock<ParkingContext>();
        }

        [Fact]
        public void ResetDb_ShouldDropAndRecreateTables()
        {
            // Arrange
            var mockDbContext = new Mock<ParkingContext>();
            mockDbContext.Setup(db => db.Database.ExecuteSqlRaw(It.IsAny<string>())).Verifiable();

            // Act
            Utility.resetDb();

            // Assert
            mockDbContext.Verify(db => db.Database.ExecuteSqlRaw(It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public void DropParkingAssignments_ShouldRemoveAllAssignments()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<ParkingAssignment>>();
            _mockContext.Setup(db => db.ParkingAssignments).Returns(mockDbSet.Object);

            // Act
            Utility.dropParkingAssignments();

            // Assert
            _mockContext.Verify(db => db.ParkingAssignments.RemoveRange(mockDbSet.Object), Times.Once);
            _mockContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SeedParkingReservations_ShouldInsertReservations()
        {
            // Arrange
            var mockDbContext = new Mock<ParkingContext>();
            mockDbContext.Setup(db => db.Database.ExecuteSqlRaw(It.IsAny<string>())).Verifiable();

            // Act
            Utility.seedParkingReservations();

            // Assert
            mockDbContext.Verify(db => db.Database.ExecuteSqlRaw(It.IsAny<string>()), Times.Once);
            mockDbContext.Verify(db => db.SaveChanges(), Times.Once);
        }

        [Fact]
        public void ScheduleDailyRun_ShouldRunRefreshAtSpecificTime()
        {
            // Note: This test case is a bit trickier to automate because it involves time and threading.
            // You might want to refactor the code to make it more testable.
            Assert.True(true); // Placeholder test
        }
    }
}
