using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using VehicleFitmentAPI.Interfaces;
using VehicleFitmentAPI.Models;

namespace VehicleFitmentAPI.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IDatabaseService _databaseService;
        public VehicleService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public List<Vehicle> GetVehicles()
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            using (var connection = _databaseService.GetConnectionString())
            {
                connection.Open();
                var command = new SqlCommand("SELECT VehicleId, Make, Model, ModelYear, Trim FROM Vehicle", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var vehicle = new Vehicle
                        {
                            VehicleId = reader.GetInt32(0),
                            Make = reader.GetString(1),
                            Model = reader.GetString(2),
                            ModelYear = reader.GetInt32(3),
                            Trim = reader.GetString(4)
                        };
                        vehicles.Add(vehicle);
                    }
                }
            }
            return vehicles;
        }

        public Vehicle InsertVehicle(Vehicle vehicle)
        {
            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                connection.Open();

                string query = "INSERT INTO VEHICLE (Make, Model, ModelYear, Trim) Values(@Make, @Model, @ModelYear, @Trim)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Make", vehicle.Make);
                    command.Parameters.AddWithValue("@Model", vehicle.Model);
                    command.Parameters.AddWithValue("@ModelYear", vehicle.ModelYear);
                    command.Parameters.AddWithValue("@Trim", vehicle.Trim);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return vehicle;
                    }
                    else
                    {
                        throw new Exception("Insert operation failed.");
                    }
                }
            }
        }

        public Vehicle GetVehicle(int id)
        {
            Vehicle vehicle = new Vehicle();

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                connection.Open();

                string query = "SELECT TOP 1 VehicleId, Make, Model, Trim, ModelYear FROM Vehicle WHERE VehicleId = @VehicleId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VehicleId", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            vehicle.VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId"));
                            vehicle.Make = reader.GetString(reader.GetOrdinal("Make"));
                            vehicle.Model = reader.GetString(reader.GetOrdinal("Model"));
                            vehicle.ModelYear = reader.GetInt32(reader.GetOrdinal("ModelYear"));
                            vehicle.Trim = reader.GetString(reader.GetOrdinal("Trim"));
                        }
                    }
                }

                return vehicle;
            }
        }

        public Vehicle UpdateVehicle(List<SqlParameter> parameters, string updateQuery, Vehicle vehicle)
        {
            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddRange(parameters.ToArray());

                    int rowsAffected = updateCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        vehicle = GetVehicle(vehicle.VehicleId);

                        return vehicle;
                    }
                    else
                    {
                        throw new Exception("Update operation failed.");
                    }
                }
            }
        }

        public int DeleteVehicle(int id)
        {
            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                connection.Open();

                string deleteFitmentsQuery = "DELETE FROM Fitment WHERE VehicleId = @VehicleId";
                
                using (SqlCommand deleteFitmentsCommand = new SqlCommand(deleteFitmentsQuery, connection))
                {
                    deleteFitmentsCommand.Parameters.AddWithValue("@VehicleId", id);
                    deleteFitmentsCommand.ExecuteNonQuery();
                }

                string deleteVehicleQuery = "DELETE FROM Vehicle WHERE VehicleId = @VehicleId";
                using (SqlCommand deleteVehicleCommand = new SqlCommand(deleteVehicleQuery, connection))
                {
                    deleteVehicleCommand.Parameters.AddWithValue("@VehicleId", id);
                    int rowsAffected = deleteVehicleCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return rowsAffected;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }
}