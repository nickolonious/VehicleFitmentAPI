﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VehicleFitmentAPI.Models
{
    public class Vehicle
    {
        public int VehicleId { get; set; }
        public string Make {  get; set; }
        public string Model { get; set; }
        public int ModelYear { get; set; }
        public string Trim { get; set; }

    }
}