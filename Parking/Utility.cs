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

        private static int MaxPermits = 50;
        private static readonly ParkingContext _db = new ParkingContext();



        public static void resetDb()
        {
            using (var db = new ParkingContext())
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
                    _db.Permits.Add(new Permit { PermitNumber = i + 1 });
                }

                _db.SaveChanges();
                db.SaveChanges();
            }


        }

        public static void dropParkingAssignments()
        {
            _db.ParkingAssignments.RemoveRange(_db.ParkingAssignments);
            _db.SaveChanges();

        }




        public static void seedParkingReservations()
        {
            int seedMax = 5_000;
            string sql = "delete from parkingReservation where true;";
            // Loop to insert reservations for 10 more days
            var startDate = DateTime.Now.AddDays(new Random().NextInt64(2, 12) * -1);
            for (int i = 1; i <= seedMax; i++) // Starting from Id 2 to 11
            {
                string startDateStr = startDate.AddDays(new Random().NextInt64(2, 7)).ToString("yyyy-MM-dd");
                string endDateStr = DateTime.Now.AddDays(new Random().NextInt64(30, 80)).ToString("yyyy-MM-dd");

                sql += $"INSERT INTO ParkingReservation ( PlateState, PlateNumber, StartDate, EndDate) " +
                             $"VALUES ('CA', {i + 1}, '{startDateStr}', '{endDateStr}');";

            }

            _db.Database.ExecuteSqlRaw(sql);
            _db.SaveChanges();
            Console.WriteLine("Inserted 10 days of parking reservations.");
        }

        public static void refreshParkingAssignments()
        {
            dropParkingAssignments();
            CreatePermitAssignmentsForReservations();
        }




        public static void CreatePermitAssignmentsForReservations()
        {
            using (var db = new ParkingContext())
            {

                // get todays reservations
                var todayReservations = _db.ParkingReservations.Where(x => x.StartDate <= DateTime.Now.Date && x.EndDate.Date >= DateTime.Now.Date);
                Console.WriteLine("raw query: " + todayReservations.ToQueryString());

                // for each parking reservation ,make and add a new parking
                foreach (var pr in todayReservations.ToList().GetRange(0, todayReservations.Count()))
                {
                    // if (_db.ParkingAssignments.Count() > 50)
                    // {
                    //     break;
                    // }

                    Permit permit = _db.Permits.FirstOrDefault(p => !p.ParkingAssignments
                                                .Any(pa => pa.ParkingReservation.StartDate.Date <= DateTime.Now.Date
                                                        && pa.ParkingReservation.EndDate.Date >= DateTime.Now.Date));

                    if (permit == null)
                    {
                        Console.WriteLine($"Error: max permits: {MaxPermits} issued for today. Admin supervision required!");
                        break;
                    }

                    // insert a new parking assignment
                    ParkingAssignment pa = new ParkingAssignment { ParkingReservationId = pr.Id, PermitId = permit.Id, date = DateTime.Now.Date };
                    _db.ParkingAssignments.Add(pa);

                    _db.SaveChanges();
                }


            }

        }

        public static void EnterNewReservations()
        {
            using (var db = new ParkingContext())
            {

                bool isInputValid = true;
                while (isInputValid)
                {
                    Console.WriteLine("Please enter quit or your new reservation details: PlateState PlateNumber StartDate EndDate");
                    var answer = Console.ReadLine();
                    var elementList = answer.Split(" ");



                    if (elementList[0].ToLower().Equals("quit"))
                    {
                        break;
                    }

                    if (elementList.Length < 4)
                    {
                        Console.WriteLine("Error: did you forget some input, check and try again\n");
                        continue;
                    }

                    //Making an assumption that user gives the correct input format
                    var startDate = DateTime.Parse(elementList[2]).Date;
                    var endDate = DateTime.Parse(elementList[3]).Date;

                    var isEndOrStartBeforeNow = startDate < DateTime.Now.Date || endDate < DateTime.Now.Date;
                    if (isEndOrStartBeforeNow)
                    {
                        Console.WriteLine("Error: state date < now || endDate < now");
                        continue;
                    }

                    ParkingReservation parkingReservation = new ParkingReservation { PlateState = elementList[0], PlateNumber = elementList[1], StartDate = startDate, EndDate = endDate };


                    bool isNow = startDate >= DateTime.Now.Date;
                    if (isNow)
                    {
                        //Permit permit = db.Permits.FirstOrDefault(r => r.Status == 0);
                        ParkingAssignment parkingAssignment;
                        using (var command = db.Database.GetDbConnection().CreateCommand())
                        {
                            string queryString2 = string.Format("SELECT * FROM Permit WHERE Status = 0 LIMIT 1;");
                            command.CommandText = queryString2;

                            db.Database.OpenConnection(); // Open the database connection
                            int permitId;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read()) // Read the first row
                                {
                                    permitId = reader.GetInt32(reader.GetOrdinal("Id"));

                                }
                                else
                                {
                                    Console.WriteLine("There are no more permits available for " + DateTime.Now.Date);
                                    isInputValid = false;
                                    continue;

                                }

                                parkingAssignment = new ParkingAssignment { ParkingReservationId = parkingReservation.Id, PermitId = permitId };

                            }


                            var sDate = parkingReservation.StartDate.Date.ToString("yyyy/MM/dd");
                            var eDate = parkingReservation.EndDate.Date.ToString("yyyy/MM/dd");


                            string queryString = String.Format(@"INSERT INTO ParkingReservation (PlateState, PlateNumber, StartDate, EndDate) VALUES ('{1}',{2},'{3}','{4}');", parkingReservation.Id, parkingReservation.PlateState, parkingReservation.PlateNumber, sDate, eDate);

                            // FIXED: This query had syntax and logical error. Fixed now. 
                            string queryString3 = String.Format(@"UPDATE ParkingAssignment set PermitId = {3}, ParkingReservationId = {4} WHERE Id = {3};", parkingAssignment.PermitId, parkingAssignment.ParkingReservationId, parkingAssignment.Id);

                            string queryString4 = String.Format(@"UPDATE Permit set Status = 1 WHERE Id = {0};", permitId);

                            foreach (var query in new string[] { queryString, queryString2, queryString3, queryString4 })
                            {
                                Console.WriteLine(query);
                            }

                            command.CommandText = queryString;
                            command.ExecuteNonQuery();

                            command.CommandText = queryString3;
                            command.ExecuteNonQuery();

                            command.CommandText = queryString4;
                            command.ExecuteNonQuery();

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

