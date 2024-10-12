using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VehicleFitmentAPI.Models
{
    public class VehicleInsert
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int ModelYear { get; set; }
    }
}