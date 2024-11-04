using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NodaTime;


namespace ParkingApp.Parking
{
    public class Utility
    {

        public static ParkingContext? dbContext = null;
        public static ParkingContext CreateDbContext()
        {       

            var options = new DbContextOptionsBuilder<ParkingContext>()
                .UseMySQL("server=localhost;database=dev_db;user=root;password=Kamala@16")
                .Options;

            return dbContext = new ParkingContext(options);
        }

        private static int MaxPermits = 50;

        public static void resetDb()
        {
            using (var db = CreateDbContext())
            {

                db.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS ParkingAssignment; DROP TABLE IF EXISTS Permit; DROP TABLE IF EXISTS ParkingReservation; ");

                var DDLcommand = @"CREATE TABLE ParkingReservation (Id int auto_increment NOT NULL, 
                                    PlateState varchar(45) DEFAULT NULL,
                                    PlateNumber varchar(45) DEFAULT NULL,
                                    StartDate date DEFAULT NULL,
                                    EndDate date DEFAULT NULL,
                                    PRIMARY KEY (Id)); ";


                db.Database.ExecuteSqlRaw(DDLcommand);
                var DDLcommand2 = @"CREATE TABLE Permit (
                                    Id int auto_increment NOT NULL,
                                    PermitNumber int DEFAULT NULL,
                                    PRIMARY KEY (Id)
                                    );";
                db.Database.ExecuteSqlRaw(DDLcommand2);



                var DDLcommand3 = @"CREATE TABLE ParkingAssignment (
                                    Id int auto_increment NOT NULL,
                                    PermitId int DEFAULT NULL,
                                    ParkingReservationId int DEFAULT NULL,
                                    date date ,
                                    PRIMARY KEY (Id),
                                    KEY PermitId_idx (PermitId),
                                    KEY ParkingReservationId_idx (ParkingReservationId),
                                    CONSTRAINT ParkingReservationId FOREIGN KEY (ParkingReservationId) REFERENCES ParkingReservation (Id),
                                    CONSTRAINT PermitId FOREIGN KEY (PermitId) REFERENCES Permit (Id)
                                    );";
                db.Database.ExecuteSqlRaw(DDLcommand3);


                // generate 50 permits
                for (int i = 0; i < MaxPermits; i++)
                {
                    db.Permits.Add(new Permit { PermitNumber = i + 1 });
                }

                db.SaveChanges();
            }


        }

        public static void dropParkingAssignments()
        {
            using (var db = CreateDbContext())
            {
                db.ParkingAssignments.RemoveRange(db.ParkingAssignments);
                db.SaveChanges();
            }
        }





        public static void seedParkingReservations()
        {
            using (var db = CreateDbContext())
            {
                int seedMax = 5_000;
                string sql = "DELETE FROM ParkingReservation WHERE TRUE;";

                // Loop to insert reservations for 10 more days
                var startDate = DateTime.Now.AddDays(new Random().NextInt64(2, 12) * -1);
                for (int i = 1; i <= seedMax; i++)
                {
                    string startDateStr = startDate.AddDays(new Random().NextInt64(2, 7)).ToString("yyyy-MM-dd");
                    string endDateStr = DateTime.Now.AddDays(new Random().NextInt64(30, 80)).ToString("yyyy-MM-dd");

                    sql += $"INSERT INTO ParkingReservation (PlateState, PlateNumber, StartDate, EndDate) " +
                           $"VALUES ('CA', {i + 1}, '{startDateStr}', '{endDateStr}');";
                }

                db.Database.ExecuteSqlRaw(sql);
                db.SaveChanges();
                Console.WriteLine("Inserted 10 days of parking reservations.");
            }
        }

        public static void refreshParkingAssignments()
        {
            dropParkingAssignments();
            CreatePermitAssignmentsForReservations();
        }




        public static void CreatePermitAssignmentsForReservations()
        {
            using (var db = CreateDbContext())
            {
                var todayReservations = db.ParkingReservations
                    .Where(x => x.StartDate <= DateTime.Now.Date && x.EndDate.Date >= DateTime.Now.Date);
                Console.WriteLine("raw query: " + todayReservations.ToQueryString());

                foreach (var pr in todayReservations.ToList())
                {
                    Permit permit = db.Permits.FirstOrDefault(p => !p.ParkingAssignments
                        .Any(pa => pa.ParkingReservation.StartDate.Date <= DateTime.Now.Date &&
                                   pa.ParkingReservation.EndDate.Date >= DateTime.Now.Date));

                    if (permit == null)
                    {
                        Console.WriteLine($"Error: max permits ({MaxPermits}) issued for today. Admin supervision required!");
                        break;
                    }

                    ParkingAssignment pa = new ParkingAssignment
                    {
                        ParkingReservationId = pr.Id,
                        PermitId = permit.Id,
                        date = DateTime.Now.Date
                    };
                    db.ParkingAssignments.Add(pa);
                    db.SaveChanges();
                }
            }
        }

        public static void EnterNewReservations()
        {
            using (var db = CreateDbContext())
            {
                bool isInputValid = true;
                while (isInputValid)
                {
                    Console.WriteLine("Please enter 'quit' or your new reservation details: PlateState PlateNumber StartDate EndDate");
                    var answer = Console.ReadLine();
                    var elementList = answer.Split(" ");

                    if (elementList[0].ToLower().Equals("quit"))
                    {
                        break;
                    }

                    if (elementList.Length < 4)
                    {
                        Console.WriteLine("Error: Did you forget some input? Check and try again.");
                        continue;
                    }

                    var startDate = DateTime.Parse(elementList[2]).Date;
                    var endDate = DateTime.Parse(elementList[3]).Date;

                    if (startDate < DateTime.Now.Date || endDate < DateTime.Now.Date)
                    {
                        Console.WriteLine("Error: Start date or end date is earlier than today.");
                        continue;
                    }

                    ParkingReservation parkingReservation = new ParkingReservation
                    {
                        PlateState = elementList[0],
                        PlateNumber = elementList[1],
                        StartDate = startDate,
                        EndDate = endDate
                    };

                    bool isNow = startDate >= DateTime.Now.Date;
                    if (isNow)
                    {
                        ParkingAssignment parkingAssignment;
                        using (var command = db.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM Permit WHERE Status = 0 LIMIT 1;";
                            db.Database.OpenConnection();
                            int permitId;

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    permitId = reader.GetInt32(reader.GetOrdinal("Id"));
                                }
                                else
                                {
                                    Console.WriteLine("There are no more permits available for " + DateTime.Now.Date);
                                    isInputValid = false;
                                    continue;
                                }

                                parkingAssignment = new ParkingAssignment
                                {
                                    ParkingReservationId = parkingReservation.Id,
                                    PermitId = permitId
                                };
                            }

                            string sDate = parkingReservation.StartDate.Date.ToString("yyyy/MM/dd");
                            string eDate = parkingReservation.EndDate.Date.ToString("yyyy/MM/dd");

                            string queryString = $"INSERT INTO ParkingReservation (PlateState, PlateNumber, StartDate, EndDate) VALUES ('{parkingReservation.PlateState}', {parkingReservation.PlateNumber}, '{sDate}', '{eDate}');";
                            string queryString3 = $"UPDATE ParkingAssignment SET PermitId = {parkingAssignment.PermitId}, ParkingReservationId = {parkingAssignment.ParkingReservationId} WHERE Id = {parkingAssignment.Id};";
                            string queryString4 = $"UPDATE Permit SET Status = 1 WHERE Id = {permitId};";

                            foreach (var query in new[] { queryString, command.CommandText, queryString3, queryString4 })
                            {
                                Console.WriteLine(query);
                                command.CommandText = query;
                                command.ExecuteNonQuery();
                            }
                        }
                    }

                    /*
                    var DDLcommand2 = @"UPDATE Permit 
                                        SET Status = 1,
                                        WHERE Id = {(0)}", result. );
                    if (permit == null){
                        Console.WriteLine("There are no more permits available for " + DateTime.Now);
                        isInputValid = false;
                        continue;
                    }
                    ParkingAssignment parkingAssignment = new ParkingAssignment{PlateState = elementList[0],PlateNumber = elementList[1], ParkingReservationId = parkingReservation.Id, PermitId = permit.Id };
                    permit.Status = 1;
                    */

                }
                db.SaveChanges();
            }

        }


        public static void scheduleDailyRun()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                if (now.Hour == 2) // Check if the current hour is 00 (midnight)
                {
                    Console.WriteLine("Running...");
                    dropParkingAssignments();
                    refreshParkingAssignments();
                    Console.WriteLine("Complete...");
                }
                Console.WriteLine($"Next run scheduled at: {DateTime.Now.AddHours(24).ToString("yyyy/MM/dd hh:mm:ss")}");

                // Sleep for 12 hours (in milliseconds)
                Thread.Sleep(12 * 60 * 60 * 1000);
            }
        }
    }
}

