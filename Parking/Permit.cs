using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingApp.Parking
{
    [Table("permit")]
    public class Permit
    {
        public int Id { get; set; }
        public int PermitNumber { get; set; }

        // 0 means available and 1 is unavailable
        // public int Status { get; set; }
        // public virtual List<Permit>? Permits { get; set; }
        // Navigation property for the related ParkingAssignments
        public virtual List<ParkingAssignment>? ParkingAssignments { get; set; }
    }
}