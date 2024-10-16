using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using VehicleFitmentAPI.Models;
using VehicleFitmentAPI.Services;

namespace VehicleFitmentAPI.Controllers
{
    public class VehicleController : ApiController
    {
        private readonly IDatabaseService _databaseService;
        private readonly IMemoryCache _memoryCache;

        public VehicleController(DatabaseService databaseService, IMemoryCache memoryCache)
        {
            _databaseService = databaseService;
            _memoryCache = memoryCache;
        }

        // GET api/<controller>
        public IHttpActionResult Get()
        {

            const string cacheKey = "GetAllVehicles";

            List<Vehicle> vehicles = _memoryCache.Get(cacheKey) as List<Vehicle>;

            if (vehicles == null)
            {

                vehicles = new List<Vehicle>();

                using (SqlConnection connection = _databaseService.GetConnectionString())
                {
                    try
                    {
                        connection.Open();

                        string query = "SELECT VehicleId, Make, Model, Trim, ModelYear FROM Vehicle";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Vehicle vehicle = new Vehicle
                                    {
                                        VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId")),
                                        Make = reader.GetString(reader.GetOrdinal("Make")),
                                        Model = reader.GetString(reader.GetOrdinal("Model")),
                                        ModelYear = reader.GetInt32(reader.GetOrdinal("ModelYear")),
                                        Trim = reader.GetString(reader.GetOrdinal("Trim"))
                                    };
                                    vehicles.Add(vehicle);
                                }

                            }
                        }
                        if (vehicles.Count > 0)
                        {
                            _memoryCache.Set(cacheKey, vehicles);
                        }

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
                }
            }

            return Ok(vehicles);
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            string cacheKey = "GetVehicleId=" + id;

            Vehicle vehicle = _memoryCache.Get(cacheKey) as Vehicle;

            if (vehicle == null)
            {
                vehicle = new Vehicle();

                using (SqlConnection connection = _databaseService.GetConnectionString())
                {
                    try
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

                        if (vehicle.VehicleId > 0)
                        {
                            _memoryCache.Set(cacheKey, vehicle);
                        }

                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
                }
            }
            return Ok(vehicle);
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody] Vehicle vehicle)
        {
            if (vehicle.Make == String.Empty || vehicle.Trim == String.Empty || vehicle.Model == String.Empty || vehicle.ModelYear <= 1930)
            {
                return BadRequest("Make, Model, and Trim must be filled out, Model Year must be greater than 1930");
            }

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
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
                            _memoryCache.Remove("GetAllVehicles");
                            return Ok(vehicle);
                        }
                        else
                        {
                            return BadRequest("Insert operation failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }

        // PUT api/<controller>
        public IHttpActionResult Put([FromBody] Vehicle vehicle)
        {
            if (vehicle.VehicleId <= 0)
            {
                return BadRequest("Invalid Vehicle ID.");
            }

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
                {
                    Vehicle existingVehicle = null;

                    string cacheKey = "GetVehicleId=" + vehicle.VehicleId;

                    existingVehicle = _memoryCache.Get(cacheKey) as Vehicle;

                    connection.Open();

                    string selectQuery = "SELECT VehicleId, Make, Model, Trim, ModelYear FROM Vehicle WHERE VehicleId = @VehicleId";

                    if (existingVehicle == null)
                    {

                        using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                        {
                            selectCommand.Parameters.AddWithValue("@VehicleId", vehicle.VehicleId);

                            using (SqlDataReader reader = selectCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    existingVehicle = new Vehicle
                                    {
                                        VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId")),
                                        Make = reader.GetString(reader.GetOrdinal("Make")),
                                        Model = reader.GetString(reader.GetOrdinal("Model")),
                                        ModelYear = reader.GetInt32(reader.GetOrdinal("ModelYear")),
                                        Trim = reader.GetString(reader.GetOrdinal("Trim"))
                                    };
                                }
                            }
                        }
                    }

                    if (existingVehicle == null)
                    {
                        return NotFound();
                    }

                    string updateQuery = "UPDATE Vehicle SET Make = @Make, Model = @Model, ModelYear = @ModelYear, Trim = @Trim WHERE VehicleId = @VehicleId";
                    using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@VehicleId", vehicle.VehicleId);
                        updateCommand.Parameters.AddWithValue("@Make", string.IsNullOrEmpty(vehicle.Make) ? existingVehicle.Make : vehicle.Make);
                        updateCommand.Parameters.AddWithValue("@Model", string.IsNullOrEmpty(vehicle.Model) ? existingVehicle.Model : vehicle.Model);
                        updateCommand.Parameters.AddWithValue("@ModelYear", vehicle.ModelYear <= 1930 ? existingVehicle.ModelYear : vehicle.ModelYear);
                        updateCommand.Parameters.AddWithValue("@Trim", string.IsNullOrEmpty(vehicle.Trim) ? existingVehicle.Trim : vehicle.Trim);

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                            {
                                selectCommand.Parameters.AddWithValue("@VehicleId", vehicle.VehicleId);

                                using (SqlDataReader reader = selectCommand.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        existingVehicle = new Vehicle
                                        {
                                            VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId")),
                                            Make = reader.GetString(reader.GetOrdinal("Make")),
                                            Model = reader.GetString(reader.GetOrdinal("Model")),
                                            ModelYear = reader.GetInt32(reader.GetOrdinal("ModelYear")),
                                            Trim = reader.GetString(reader.GetOrdinal("Trim"))
                                        };
                                    }
                                }
                            }

                            _memoryCache.Set(cacheKey, existingVehicle);

                            return Ok(existingVehicle);
                        }
                        else
                        {
                            return BadRequest("Update operation failed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }

        // DELETE api/<controller>/5
        public IHttpActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Vehicle ID.");
            }

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
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
                            //Better to remove the individual vehicle from cache, but this is simpler
                            _memoryCache.Remove("GetAllVehicles");
                            _memoryCache.Remove("GetVehicleId=" + id);
                            return Ok("Vehicle and associated fitments deleted!");
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
        }

    }
}