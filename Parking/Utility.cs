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
        private static readonly ParkingContext _db = new ParkingContext();



        public static void resetDb()
        {
            using (var db = new ParkingContext())
            {

                db.Database.ExecuteSqlRaw("DROP TABLE IF EXISTS ParkingAssignment; DROP TABLE IF EXISTS Permit; DROP TABLE IF EXISTS ParkingReservation;");

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
                                    PlateState varchar(45) DEFAULT NULL,
                                    PlateNumber varchar(45) DEFAULT NULL,
                                    PermitId int DEFAULT NULL,
                                    ParkingReservationId int DEFAULT NULL,
                                    PRIMARY KEY (Id),
                                    KEY PermitId_idx (PermitId),
                                    KEY ParkingReservationId_idx (ParkingReservationId),
                                    CONSTRAINT ParkingReservationId FOREIGN KEY (ParkingReservationId) REFERENCES ParkingReservation (Id),
                                    CONSTRAINT PermitId FOREIGN KEY (PermitId) REFERENCES Permit (Id)
                                    );";
                db.Database.ExecuteSqlRaw(DDLcommand3);



                //db.SaveChanges();
            }


        }

        public static void dropParkingAssignments()
        {
            // _db.ParkingAssignments.RemoveRange(_db.ParkingAssignments);
            // _db.SaveChanges();
            _db.Database.ExecuteSqlRaw("DELETE FROM ParkingAssignment;");
        }

        public static void RemoveExpiredReservations()
        {
            using (var db = new ParkingContext())
            {
                var query = from parkingAssignment in db.ParkingAssignments

                            select parkingAssignment;

                List<ParkingAssignment> toBeDeleted = [];


                //loop over parking assignment
                // foreach (ParkingAssignment parkingAssignment in db.ParkingAssignments)
                // {
                //     var prId = parkingAssignment.ParkingReservationId;
                //     var permitId = parkingAssignment.PermitId;

                //     // for parkingAssignment.reservations ,
                //     ParkingReservation reservation = (ParkingReservation)db.ParkingReservations.Where(x => x.Id == prId).Select(x => x);
                //     var endDate = reservation.EndDate;
                //     //var date = LocalDateTime.FromDateTime(DateTime.Now).Date;
                //     //var endDate1 = LocalDateTime.FromDateTime(endDate).Date;

                //     // if expired remove reservation
                //     if (DateTime.Now > endDate)
                //     {

                //         //find permit related to parking assignment
                //         Permit permit = (Permit)db.Permits.Where(x => x.Id == permitId).Select(x => x);
                //         // permit.Status = 0; //maker permit as available

                //         // remove related assignments
                //         toBeDeleted.Add(parkingAssignment);

                //         db.ParkingReservations.Remove(reservation);
                //         db.SaveChanges();

                //     }
                // }
                // foreach (ParkingAssignment parkingAssignment in toBeDeleted)
                // {
                //     db.ParkingAssignments.Remove(parkingAssignment);
                // }
            }


        }

        public static void refreshParkingAssignments(){
            dropParkingAssignments();
            CreatePermitAssignmentsForReservations();
        }


        public static void CreatePermitAssignmentsForReservations()
        {
            using (var db = new ParkingContext())
            {
                           
                List<ParkingReservation> parkingReservations = new List<ParkingReservation>();
                string queryToSetAllReservations = string.Format("SELECT * FROM ParkingReservation;");
                using (var command = db.Database.GetDbConnection().CreateCommand()) {
                    command.CommandText = queryToSetAllReservations;
                    db.Database.OpenConnection();
                    using (var reader = command.ExecuteReader()) {
                        while(reader.Read()) {
                            var id = reader.GetInt32("Id");
                            var state = reader.GetString("PlateState");
                            var plate = reader.GetString("PlateNumber");
                            var start = reader.GetDateTime("StartDate");
                            var end = reader.GetDateTime("EndDate");
                            Console.WriteLine("ID = " + start);
                            var reservation = new ParkingReservation { Id = id, PlateState = state, PlateNumber = plate, StartDate = start, EndDate = end};
                            parkingReservations.Add(reservation);
                        }
                    }
                }
                Console.WriteLine("ALL Reservations");
                Console.WriteLine(parkingReservations);
                // var parkingReservations = (from parkingReservation in db.ParkingReservations select parkingReservation).ToList();
                var AssignmentsMax = 50;

                // permit for 2024-11-1 2024-11-7, [p11-1:{1,..50}],...,p11-7{1,50})
                foreach (ParkingReservation parkingReservation in parkingReservations)
                {
                    var startDate = parkingReservation.StartDate;
                    var isReservationInvalid = parkingReservation.StartDate > DateTime.Now.Date || parkingReservation.EndDate < DateTime.Now.Date;
                    var assignmentsCreatedCount = db.ParkingAssignments?.Count();

                    if (assignmentsCreatedCount > AssignmentsMax)
                    {
                        // alert the manager
                        Console.WriteLine("Warning: max assignments issued");
                        break;
                    }


                    if (isReservationInvalid)
                    {
                        continue;
                    }

                    //Retrieve first permit without any parking assignment

                    // Permit permit = db.Permits.First(r =>r.);
                    List<Permit> permits = new List<Permit>();
                    string queryToGetAllPermits = string.Format("SELECT * FROM Permit;");
                    using (var command = db.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = queryToGetAllPermits;
                        db.Database.OpenConnection();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var id = reader.GetInt32("Id");
                                var permitNum = reader.GetInt32("PermitNumber");
      
                                var currentPermit = new Permit { Id = id, PermitNumber = permitNum };
                                permits.Add(currentPermit);
                            }
                        }
                    }

                    // Before Permit permit = db.Permits.FirstOrDefault(r => r.ParkingAssignments == null || !r.ParkingAssignments.Any());
                    Permit permit = permits.FirstOrDefault(r => r.ParkingAssignments == null || !r.ParkingAssignments.Any());

                    if (permit == null)
                    {
                        Console.WriteLine("There are no more permits available for " + DateTime.Now.Date);
                        break;
                    }

                    // permit.Status = 1;
                    //Create reservation, i.e, Parking Assignment
                    var newParkingAssignment = new ParkingAssignment { PermitId = permit.Id, ParkingReservationId = parkingReservation.Id, PlateState = parkingReservation.PlateState, PlateNumber = parkingReservation.PlateNumber };

                    db.SaveChanges();

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

                                parkingAssignment = new ParkingAssignment { PlateState = elementList[0], PlateNumber = elementList[1], ParkingReservationId = parkingReservation.Id, PermitId = permitId };

                            }


                            var sDate = parkingReservation.StartDate.Date.ToString("yyyy/MM/dd");
                            var eDate = parkingReservation.EndDate.Date.ToString("yyyy/MM/dd");


                            string queryString = String.Format(@"INSERT INTO ParkingReservation (PlateState, PlateNumber, StartDate, EndDate) VALUES ('{1}',{2},'{3}','{4}');", parkingReservation.Id, parkingReservation.PlateState, parkingReservation.PlateNumber, sDate, eDate);

                            // FIXED: This query had syntax and logical error. Fixed now. 
                            string queryString3 = String.Format(@"UPDATE ParkingAssignment set Id = {0}, PlateState = '{1}', PlateNumber = {2}, PermitId = {3}, ParkingReservationId = {4} WHERE PermitId = {3};", parkingAssignment.Id, parkingAssignment.PlateState, parkingAssignment.PlateNumber, parkingAssignment.PermitId, parkingAssignment.ParkingReservationId);

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
    }
}

