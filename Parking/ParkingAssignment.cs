using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ParkingApp.Parking
{
    [Table("ParkingAssignment")]
    public class ParkingAssignment
    {
        [Key] // Marks this property as the primary key
        [Column("Id")] // Maps the property to the "Id" column in the database
        public int Id { get; set; }
        public string? PlateState { get; set; }
        public string? PlateNumber { get; set; }

        public int PermitId { get; set; }

        public int ParkingReservationId { get; set; }


        public virtual List<ParkingAssignment>? ParkingAssignments { get; set; }
        public virtual Permit? Permit { get; set; }
    }
}