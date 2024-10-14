using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VehicleFitmentAPI.Models;

namespace VehicleFitmentAPI.Controllers
{
    public class PartsController : ApiController
    {
        // GET api/<controller>

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;
            List<Part> parts = new List<Part>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT PartId, PartsNumber, PartsName, Description, ImageUrl FROM Part";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Part part = new Part
                                {
                                    PartId = reader.GetInt32(reader.GetOrdinal("PartId")),
                                    PartsNumber = reader.GetInt32(reader.GetOrdinal("PartsNumber")),
                                    PartsName = reader.GetString(reader.GetOrdinal("Description")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                                };
                                parts.Add(part);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
            return Ok(parts);
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;
            Part part = new Part();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT TOP 1 PartId, PartsNumber, PartsName, Description, ImageUrl FROM Part WHERE PartId = @PartId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PartId", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                part.PartId = reader.GetInt32(reader.GetOrdinal("PartId"));
                                part.PartsNumber = reader.GetInt32(reader.GetOrdinal("PartsNumber"));
                                part.PartsName = reader.GetString(reader.GetOrdinal("PartsName"));
                                part.Description = reader.GetString(reader.GetOrdinal("Description"));
                                part.ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl"));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
            }
            return Ok(part);
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody] Part part)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;

            if (part.PartsNumber == 0 || part.PartsName == String.Empty || part.Description == String.Empty || part.ImageUrl == String.Empty)
            {
                return BadRequest("PartsNumber, PartsName, Description, and ImageUrl must be filled out");
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO Part (PartsNumber, PartsName, Description, ImageUrl) Values(@PartsNumber, @PartsName, @Description, @ImageUrl)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PartsNumber", part.PartsNumber);
                        command.Parameters.AddWithValue("@PartsName", part.PartsName);
                        command.Parameters.AddWithValue("@Description", part.Description);
                        command.Parameters.AddWithValue("@ImageUrl", part.ImageUrl);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Ok("Part inserted!");
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
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}