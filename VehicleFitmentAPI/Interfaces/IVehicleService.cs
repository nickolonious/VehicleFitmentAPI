using System.Collections.Generic;
using System.Data.SqlClient;
using VehicleFitmentAPI.Models;

namespace VehicleFitmentAPI.Interfaces
{
    public interface IVehicleService
    {
        List<Vehicle> GetVehicles();
        Vehicle InsertVehicle(Vehicle vehicle);
        Vehicle GetVehicle(int id);
        Vehicle UpdateVehicle(List<SqlParameter> parameters, string updateQuery, Vehicle vehicle);
        int DeleteVehicle(int id);
    }
}