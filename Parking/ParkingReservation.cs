using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace ParkingApp.Parking
{
    [Table("parkingReservation")]
    public class ParkingReservation
    {
       
        public int Id { get; set; }
        public string? PlateState { get; set; }
        public string? PlateNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        // public virtual List<ParkingReservation>? ParkingReservations { get; set; }




    }
}