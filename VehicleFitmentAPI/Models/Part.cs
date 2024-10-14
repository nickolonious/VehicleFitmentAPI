﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VehicleFitmentAPI.Models
{
    public class Part
    {
        public int PartId {  get; set; }
        public int PartsNumber {  get; set; }
        public string PartsName {  get; set; }
        public string Description {  get; set; }
        public string ImageUrl {  get; set; }

    }
}