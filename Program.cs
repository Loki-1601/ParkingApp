// See https://aka.ms/new-console-template for more information
using Mysqlx.Crud;
using ParkingApp.Parking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
// Example in Program.cs

var options = new DbContextOptionsBuilder<ParkingContext>()
            .UseMySQL("server=localhost;database=dev_db;user=root;password=Kamala@16")
            .Options;

        using (var pc = new ParkingContext(options))
        {
            bool isInputValid = false;
            while (!isInputValid)
            {
                Console.WriteLine("Do you want to start off with zero reservations and assignments?");
                var answer = Console.ReadLine().ToLower();
                if (answer.Equals("yes"))
                {
                    Utility.resetDb();
                    
                    // Insert 50 permits using DbContext
                    for (int i = 1; i <= 50; i++)
                    {
                        pc.Permits.Add(new Permit { PermitNumber = i });
                    }
                    pc.SaveChanges(); // Save all permits to the database

                    break;
                }
                else if (answer.Equals("no"))
                {
                    break;
                }
            }
        }


/*
var permit1 = new Permit { Id = 1, PermitNumber = 1, Status = 0 };
pc.Permit.Add(permit1);
*/



// Create a map of options using strings as keys
        var optionsMap = new Dictionary<string, Action>
        {
            { "0", Utility.resetDb },
            { "1", Utility.seedParkingReservations },
            { "2", Utility.refreshParkingAssignments },
            { "3", Utility.CreatePermitAssignmentsForReservations },
            { "4", Utility.EnterNewReservations },
            { "5", Utility.scheduleDailyRun }
        };

    // Display options to the user
            Console.WriteLine("Select an option:");
            foreach (var option in optionsMap)
            {
                Console.WriteLine($"{option.Key}: {option.Value.Method.Name}");
            }

            // Get user input
            string choice = Console.ReadLine();
            if (optionsMap.ContainsKey(choice))
            {
                Console.WriteLine("User choice: " + choice);
                // Execute the selected function
                optionsMap[choice].Invoke();
            }
            else
            {
                Console.WriteLine("Invalid selection. Please try again.");
            }
        
    
