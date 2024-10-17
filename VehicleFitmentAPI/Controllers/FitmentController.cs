using Microsoft.Extensions.Caching.Memory;
using System;
using System.Data.SqlClient;
using System.Web.Http;
using VehicleFitmentAPI.Models;
using VehicleFitmentAPI.Services;

namespace VehicleFitmentAPI.Controllers
{
    public class FitmentController : ApiController
    {

        private readonly IDatabaseService _databaseService;
        private readonly IMemoryCache _memoryCache;

        public FitmentController(DatabaseService databaseService, IMemoryCache memoryCache)
        {
            _databaseService = databaseService;
            _memoryCache = memoryCache;
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody] Fitment fitment)
        {
            if (fitment.PartId == 0 || fitment.VehicleId == 0)
            {
                return BadRequest("PartId and VehicleId are required");
            }

            using (SqlConnection connection = _databaseService.GetConnectionString())
            {
                try
                {
                    connection.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Fitment WHERE PartId = @PartId AND VehicleId = @VehicleId";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@PartId", fitment.PartId);
                        checkCommand.Parameters.AddWithValue("@VehicleId", fitment.VehicleId);

                        int count = (int)checkCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            return BadRequest("Fitment record already exists.");
                        }
                    }

                    string insertQuery = "INSERT INTO Fitment (PartId, VehicleId) VALUES (@PartId, @VehicleId)";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@PartId", fitment.PartId);
                        insertCommand.Parameters.AddWithValue("@VehicleId", fitment.VehicleId);

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            _memoryCache.Remove("GetPartsByVehicleId=" + fitment.VehicleId);
                            return Ok("Fitment inserted successfully.");
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

        // GET api/<controller>
        // The Gets did not end up being used in this app, were not needed but kept them around to show work done

        //public IHttpActionResult Get()
        //{
        //    List<FitmentView> fitments = new List<FitmentView>();

        //    using (SqlConnection connection = _databaseService.GetConnectionString())
        //    {
        //        try
        //        {
        //            connection.Open();

        //            string query = "SELECT f.FitmentID, v.Make, v.Model, v.ModelYear, v.Trim, p.PartsName, p.PartsNumber FROM Fitment f JOIN Vehicle v ON f.VehicleID = v.VehicleID JOIN Part p ON f.PartID = p.PartID" ;

        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    while (reader.Read())
        //                    {
        //                        FitmentView fitmentView = new FitmentView
        //                        {
        //                            FitmentID = reader.GetInt32(reader.GetOrdinal("FitmentID")),
        //                            Vehicle = reader.GetString(reader.GetOrdinal("Make")) + " " + reader.GetString(reader.GetOrdinal("Model")) + " " + reader.GetInt32(reader.GetOrdinal("ModelYear")) + " " + reader.GetString(reader.GetOrdinal("Trim")),
        //                            PartName = reader.GetString(reader.GetOrdinal("PartsName")),
        //                            PartNumber = reader.GetInt32(reader.GetOrdinal("PartsNumber")),
        //                        };
        //                        fitments.Add(fitmentView);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return InternalServerError(ex);
        //        }
        //    }
        //    return Ok(fitments);
        //}

        //// GET api/<controller>/5
        //public IHttpActionResult Get(int id)
        //{
        //    FitmentView fitment = new FitmentView();

        //    using (SqlConnection connection = _databaseService.GetConnectionString())
        //    {
        //        try
        //        {
        //            connection.Open();

        //            string query = "SELECT TOP 1 f.FitmentID, v.Make, v.Model, v.ModelYear, v.Trim, p.PartsName, p.PartsNumber FROM Fitment f JOIN Vehicle v ON f.VehicleID = v.VehicleID JOIN Part p ON f.PartID = p.PartID WHERE FitmentID = @FitmentID";

        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@FitmentID", id);

        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    fitment.FitmentID = reader.GetInt32(reader.GetOrdinal("FitmentID"));
        //                    fitment.Vehicle = reader.GetString(reader.GetOrdinal("Make")) + " " + reader.GetString(reader.GetOrdinal("Model")) + " " + reader.GetInt32(reader.GetOrdinal("ModelYear")) + " " + reader.GetString(reader.GetOrdinal("Trim"));
        //                    fitment.PartName = reader.GetString(reader.GetOrdinal("PartsName"));
        //                    fitment.PartNumber = reader.GetInt32(reader.GetOrdinal("PartsNumber"));
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return InternalServerError(ex);
        //        }
        //    }
        //    return Ok(fitment);
        //}
    }
}