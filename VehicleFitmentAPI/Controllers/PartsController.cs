﻿using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Http;
using VehicleFitmentAPI.Models;
using VehicleFitmentAPI.Services;
using Azure.Storage.Blobs;
using System.Collections;


namespace VehicleFitmentAPI.Controllers
{
    public class PartsController : ApiController
    {
        private readonly IDatabaseService _databaseService;
        private readonly IMemoryCache _memoryCache;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _environment;

        public PartsController(DatabaseService databaseService, IMemoryCache memoryCache, BlobServiceClient blobServiceClient)
        {
            _databaseService = databaseService;
            _memoryCache = memoryCache;
            _blobServiceClient = blobServiceClient;
            _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
        }

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            const string cacheKey = "GetAllParts";

            List<Part> parts = _memoryCache.Get(cacheKey) as List<Part>;

            if (parts == null)
            {
                parts = new List<Part>();

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
                _memoryCache.Set(cacheKey, parts);
            }

            return Ok(parts);
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            string cacheKey = "GetPartId=" + id;

            Part part = _memoryCache.Get(cacheKey) as Part;

            if (part == null)
            {
                part = new Part();

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

                        if(part.PartId > 0)
                        {
                            _memoryCache.Set(cacheKey, part);
                        }
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(ex);
                    }
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

            string imageUrl;

            if (_environment == "Production")
            {
                var fileName = Path.GetFileName(uploadedFile.FileName);
                var blobContainer = _blobServiceClient.GetBlobContainerClient("partsimages");

                blobContainer.CreateIfNotExists();

                var blobClient = blobContainer.GetBlobClient(fileName);

                try
                {
                    using (var stream = uploadedFile.InputStream)
                    {
                        blobClient.Upload(stream, true);
                    }
                    imageUrl = blobClient.Uri.ToString();
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("Error saving the image file to Blob Storage: " + ex.Message));
                }
            }
            else
            {
                var filePath = HttpContext.Current.Server.MapPath("~/Images/" + uploadedFile.FileName);
                try
                {
                    uploadedFile.SaveAs(filePath);
                    imageUrl = "/Images/" + uploadedFile.FileName;
                }
                catch (Exception ex)
                {
                    return InternalServerError(new Exception("Error saving the image file locally: " + ex.Message));
                }
            }

            var newPart = new Part
            {
                PartsNumber = partsNumber,
                PartsName = partsName,
                Description = description,
                ImageUrl = imageUrl
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
                            _memoryCache.Remove("GetAllParts");

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
            
            string cacheKey = "GetPartsByVehicleId=" + vehicleId;

            List<Part> parts = _memoryCache.Get(cacheKey) as List<Part>;

            if (parts == null)
            {
                parts = new List<Part>();

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
            }
            return Ok(parts);
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put()
        {
            var httpRequest = HttpContext.Current.Request;

            if (!int.TryParse(httpRequest.Form["PartId"], out int partId))
            {
                return BadRequest("PartId must be an Integer");
            }

            if (!int.TryParse(httpRequest.Form["PartsNumber"], out int partsNumber))
            {
                return BadRequest("Parts Number must be an Integer");
            }

            if (partId == 0)
            {
                return BadRequest("PartId must be provided");
            }

            var partsName = httpRequest.Form["PartsName"];
            var description = httpRequest.Form["Description"];
            var uploadedFile = httpRequest.Files.Count > 0 ? httpRequest.Files[0] : null;

            string imageUrl = null;
            
            if (uploadedFile != null)
            {
                if (_environment == "Production")
                {
                    var fileName = Path.GetFileName(uploadedFile.FileName);
                    var blobContainer = _blobServiceClient.GetBlobContainerClient("partsimages");
                    blobContainer.CreateIfNotExists();

                    var blobClient = blobContainer.GetBlobClient(fileName);

                    try
                    {
                        using (var stream = uploadedFile.InputStream)
                        {
                            blobClient.Upload(stream, true);
                        }
                        imageUrl = blobClient.Uri.ToString();
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(new Exception("Error saving the image file to Blob Storage: " + ex.Message));
                    }
                }
                else
                {
                    var filePath = HttpContext.Current.Server.MapPath("~/Images/" + uploadedFile.FileName);
                    try
                    {
                        uploadedFile.SaveAs(filePath);
                        imageUrl = "/Images/" + uploadedFile.FileName;
                    }
                    catch (Exception ex)
                    {
                        return InternalServerError(new Exception("Error saving the image file locally: " + ex.Message));
                    }
                }
            }

            var updateFields = new List<string>();
            var parameters = new List<SqlParameter>();

            if (partsNumber != 0)
            {
                updateFields.Add("PartsNumber = @PartsNumber");
                parameters.Add(new SqlParameter("@PartsNumber", partsNumber));
            }

            if (!string.IsNullOrEmpty(partsName))
            {
                updateFields.Add("PartsName = @PartsName");
                parameters.Add(new SqlParameter("@PartsName", partsName));
            }

            if (!string.IsNullOrEmpty(description))
            {
                updateFields.Add("Description = @Description");
                parameters.Add(new SqlParameter("@Description", description));
            }

            if (!string.IsNullOrEmpty(imageUrl))
            {
                updateFields.Add("ImageUrl = @ImageUrl");
                parameters.Add(new SqlParameter("@ImageUrl", imageUrl));
            }

            if (updateFields.Count == 0)
            {
                return BadRequest("No fields to update");
            }

            var updateQuery = "UPDATE Part SET " + string.Join(", ", updateFields) + " WHERE PartId = @PartId";
            parameters.Add(new SqlParameter("@PartId", partId));

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            _memoryCache.Remove("GetAllParts");
                            _memoryCache.Remove("GetPartById=" + partId);

                            var fitmentQuery = "SELECT * FROM FITMENT WHERE PartId = @PartId AND VehicleId IN (SELECT DISTINCT VehicleId FROM FITMENT WHERE PartId = @PartId)";
                            List<Fitment> matchingFitments = new List<Fitment>();
                            using (SqlCommand fitmentCommand = new SqlCommand(fitmentQuery, connection))
                            {
                                fitmentCommand.Parameters.AddWithValue("@PartId", partId);

                                using (SqlDataReader reader = fitmentCommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        Fitment fitment = new Fitment
                                        {
                                            FitmentId = reader.GetInt32(reader.GetOrdinal("FitmentId")),
                                            VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId"))
                                        };
                                    }
                                }
                            }

                            foreach (Fitment fitment in matchingFitments)
                            {
                                _memoryCache.Remove("GetPartsByVehicleId=" + fitment.VehicleId);
                            }

                            return Ok("Part updated successfully");
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

                    string deleteFitmentsQuery = "DELETE FROM Fitment WHERE PartId = @PartId";
                    using (SqlCommand deleteFitmentsCommand = new SqlCommand(deleteFitmentsQuery, connection))
                    {
                        deleteFitmentsCommand.Parameters.AddWithValue("@PartId", id);
                        deleteFitmentsCommand.ExecuteNonQuery();
                    }

                    string deletePartQuery = "DELETE FROM Part WHERE PartId = @PartId";
                    using (SqlCommand deleteVehicleCommand = new SqlCommand(deletePartQuery, connection))
                    {
                        deleteVehicleCommand.Parameters.AddWithValue("@PartId", id);

                        int rowsAffected = deleteVehicleCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            _memoryCache.Remove("GetAllParts");
                            _memoryCache.Remove("GetPartId=" + id);
                            return Ok("Part and associated Fitments deleted!");
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