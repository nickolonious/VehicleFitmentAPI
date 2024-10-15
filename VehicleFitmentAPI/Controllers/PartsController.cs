using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Http;
using VehicleFitmentAPI.Models;
using VehicleFitmentAPI.Services;

namespace VehicleFitmentAPI.Controllers
{
    public class PartsController : ApiController
    {

        private readonly IDatabaseService _databaseService;

        public PartsController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            List<Part> parts = new List<Part>();

            using (SqlConnection connection = _databaseService.GetConnectionString())
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
                                    PartsName = reader.GetString(reader.GetOrdinal("PartsName")),
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

            Part part = new Part();

            using (SqlConnection connection = _databaseService.GetConnectionString())
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
        public IHttpActionResult Post()
        {
            var httpRequest = HttpContext.Current.Request;
            
            var uploadedFile = httpRequest.Files[0];
            var partsName = httpRequest.Form["PartsName"];
            var description = httpRequest.Form["Description"];

            if (!int.TryParse(httpRequest.Form["PartsNumber"], out int partsNumber))
            {
                return BadRequest("Parts Number must be an Integer");
            }

            if (partsNumber == 0 || partsName == String.Empty || description == String.Empty 
                || (httpRequest.Files.Count == 0 || uploadedFile == null || uploadedFile.ContentLength == 0))
            {
                return BadRequest("PartsNumber, PartsName, Description, and Image must be filled out");
            }

            var fileName = Path.GetFileName(uploadedFile.FileName);
            var filePath = HttpContext.Current.Server.MapPath("~/Images/PartsImages/" + fileName);
            uploadedFile.SaveAs(filePath);

            try
            {
                var directoryPath = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                uploadedFile.SaveAs(filePath);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Error saving the image file: " + ex.Message));
            }

            var newPart = new Part
            {
                PartsNumber = partsNumber,
                PartsName = partsName,
                Description = description,
                ImageUrl = "/Images/PartsImages/" + uploadedFile.FileName
            };

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
                {
                    connection.Open();

                    string query = "INSERT INTO Part (PartsNumber, PartsName, Description, ImageUrl) Values(@PartsNumber, @PartsName, @Description, @ImageUrl)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PartsNumber", newPart.PartsNumber);
                        command.Parameters.AddWithValue("@PartsName", newPart.PartsName);
                        command.Parameters.AddWithValue("@Description", newPart.Description);
                        command.Parameters.AddWithValue("@ImageUrl", newPart.ImageUrl);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            //TODO: Update the view to show new part
                            return Ok(newPart);
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

        // GET api/parts/{vehicleId}/parts
        [HttpGet]
        [Route("api/parts/{vehicleId}/parts")]
        public IHttpActionResult GetPartsByVehicleId(int vehicleId)
        {
            List<Part> parts = new List<Part>();

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
                {
                    connection.Open();

                    string query = @"
                        SELECT p.PartId, p.PartsNumber, p.PartsName, p.Description, p.ImageUrl
                        FROM Part p
                        INNER JOIN Fitment f ON p.PartId = f.PartId
                        WHERE f.VehicleId = @VehicleId";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@VehicleId", vehicleId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Part part = new Part
                                {
                                    PartId = reader.GetInt32(reader.GetOrdinal("PartId")),
                                    PartsNumber = reader.GetInt32(reader.GetOrdinal("PartsNumber")),
                                    PartsName = reader.GetString(reader.GetOrdinal("PartsName")),
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
    }
}