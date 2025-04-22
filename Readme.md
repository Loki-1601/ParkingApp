# Asp C# console parking reservation permit app

### Learnings
1. Navigate to the project directory: `cd ParkingApp`.
2. Run tests: `dotnet test`.
3. Be cautious with `virtual List<ClassSelf>` to avoid phantom `classId` bugs.
4. Start the app from the console: `dotnet run`.
5. Optionally build an executable: `dotnet publish -c Release -r win-x64 --self-contained`.

### Project Structure and Hierarchy

Permits

ParkingAssignment (on some date x, you have permit y to park at any open lot)


ParkingReservations


---------
console.
1. presents systems
2. allows user to select system options
3. options link to static functtions in Utility class
4. utility class use ParkingContext to manage db operations
    { "0", Utility.resetDb },
    { "1", Utility.seedParkingReservations }, //populate with test data
    { "2", Utility.refreshParkingAssignments }, //drop assignments and      rebuild assignments for today x day
    { "3", Utility.CreatePermitAssignmentsForReservations },
    { "4", Utility.EnterNewReservations },
    { "5", Utility.scheduleDailyRun }


What is efcore and linq
### EF Core and LINQ Overview

**EF Core (Entity Framework Core):**
- EF Core is an Object-Relational Mapper (ORM) for .NET.
- It allows developers to work with a database using .NET objects, eliminating the need for most data-access code.
- Supports LINQ queries, change tracking, and schema migrations.
- Common commands:
    - `dotnet ef migrations add <MigrationName>`: Adds a new migration.
    - `dotnet ef database update`: Updates the database to the latest migration.

**LINQ (Language Integrated Query):**
- LINQ is a query syntax in .NET for querying collections like arrays, lists, or databases.
- Works seamlessly with EF Core to query databases.
- Example LINQ query:
    ```csharp
    var permits = context.Permits
                                             .Where(p => p.IsActive)
                                             .OrderBy(p => p.ExpirationDate)
                                             .ToList();
    ```
- LINQ supports filtering, ordering, grouping, and joining data.

### How EF Core and LINQ Work Together
- EF Core translates LINQ queries into SQL commands executed on the database.
- This integration simplifies data access and manipulation in .NET applications.
- Example:
    ```csharp
    var reservations = context.ParkingReservations
                                                        .Where(r => r.Date == DateTime.Today)
                                                        .ToList();
    ```
- This query fetches all parking reservations for the current day.


# NBs
- Linq: writing expresive queries, basically queries in simple syntax without worrying about loops , ifs and normal syntax overhead, no sql for crud
    eg  Linq=> db.Permit.FindById(3)
    - sql db.query(`select * from permits where id = `+ 3);

- efcore: (orm => object relational mapping) methodology of accessig and managing data from db based on entities eg (Car for Cars, Permit for Permits) and its relations
  eg Permit.PermitAssignments
  Car.Owner => = select * from owner where id=car.owner_id
  TotalParkingAmount= ParkingReservation.FindById(4).Permit.ParkingAssignments.Count() * 10;
### SQL Equivalent for `Permit.FindById(4).ParkingAssignments.Count()`
<!-- ParkingRes.Permit.ParkingAssignment -->

The equivalent SQL query for counting the parking assignments of a permit with ID 4 would be:

```sql
SELECT COUNT(*)
FROM ParkingAssignments
WHERE PermitId = 4;
```

This query counts the number of rows in the `ParkingAssignments` table where the `PermitId` matches 4.