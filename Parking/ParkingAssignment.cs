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

        public int Id { get; set; }
        public int PermitId { get; set; }

        public int ParkingReservationId { get; set; }

        public DateTime date { get; set; }

        public virtual ParkingReservation? ParkingReservation { get; set; }
        public virtual Permit? Permit { get; set; }
    }
}