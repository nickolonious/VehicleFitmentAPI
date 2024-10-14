using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VehicleFitmentAPI.Models
{
    public class Fitment
    {
        public int FitmentId { get; set; }
        public int VehicleId { get; set; }
        public int PartId { get; set; }

    }
}