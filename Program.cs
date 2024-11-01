// See https://aka.ms/new-console-template for more information
using Mysqlx.Crud;
using ParkingApp.Parking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

var pc = new ParkingContext();


/*
var permit1 = new Permit { Id = 1, PermitNumber = 1, Status = 0 };
pc.Permit.Add(permit1);
*/

bool isInputValid = false;
while (!isInputValid)
{
    Console.WriteLine("Do you want to start off with zero reservations and assignments?");
    var answer = Console.ReadLine().ToLower();
    if (answer.Equals("yes"))
    {
        Utility.resetDb();
        var insert50 = "";
        for (int i = 0; i <= 49; i++)
        {
            int PermitNumber = i + 1;
            insert50 += string.Format("INSERT INTO  Permit ( PermitNumber ) VALUES ( {0});", PermitNumber);
        }

        pc.Database.ExecuteSqlRaw(insert50);

        break;
    }
    else if (answer.Equals("no"))
    {
        break;
    }
}


// Create a map of options using strings as keys
while (true)
{

    var options = new Dictionary<string, Action>
        {
            { "1", Utility.RemoveExpiredReservations },
            { "2", Utility.CreatePermitAssignmentsForReservations },
            { "3", Utility.EnterNewReservations },
            { "4", Utility.refreshParkingAssignments }
        };

    // Display options to the user using a loop
    Console.WriteLine("Select an option:");
    foreach (var option in options)
    {
        Console.WriteLine($"{option.Key}: {option.Value.Method.Name}");
    }

    // Get user input
    string choice = Console.ReadLine();
    if (options.ContainsKey(choice))
    {
        Console.WriteLine("user choice: " + choice);
        // Execute the selected function
        options[choice].Invoke();
    }
    else
    {
        Console.WriteLine("Invalid selection. Please try again.");
    }



}
