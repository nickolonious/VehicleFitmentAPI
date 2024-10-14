using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using VehicleFitmentAPI.Models;

namespace VehicleFitmentAPI.Controllers
{
    public class VehicleController : ApiController
    {

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;
            List<Vehicle> vehicles = new List<Vehicle>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT VehicleId, Make, Model, ModelYear FROM Vehicle";

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
                                    ModelYear = reader.GetInt32(reader.GetOrdinal("ModelYear"))
                                };
                                vehicles.Add(vehicle);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
            return Ok(vehicles);
        }


        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;
            Vehicle vehicle = new Vehicle();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT TOP 1 VehicleId, Make, Model, ModelYear FROM Vehicle WHERE VehicleId = @VehicleId";

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
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
            return Ok(vehicle);
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody] Vehicle vehicle)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;

            if (vehicle.Make == String.Empty || vehicle.Model == String.Empty || vehicle.ModelYear <= 1930)
            {
                return BadRequest("Make and Model must be filled out, Model Year must be greater than 1930");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    
                    string query = "INSERT INTO VEHICLE (Make, Model, ModelYear) Values(@Make, @Model, @ModelYear)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Make", vehicle.Make);
                        command.Parameters.AddWithValue("@Model", vehicle.Model);
                        command.Parameters.AddWithValue("@ModelYear", vehicle.ModelYear);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok("Vehicle inserted!");
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

        // PUT api/<controller>/5
        //public void Put(int id, [FromBody] string value)
        //{

        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{

        //}
    }
}