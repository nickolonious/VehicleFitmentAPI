using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Http;
using VehicleFitmentAPI.Interfaces;
using VehicleFitmentAPI.Models;

namespace VehicleFitmentAPI.Controllers
{
    public class VehicleController : ApiController
    {
        private readonly ICacheService _cacheService;
        private readonly IVehicleData _vehicleData;

        public VehicleController(ICacheService cacheService, IVehicleData vehicleData)
        {
            _cacheService = cacheService;
            _vehicleData = vehicleData;
        }

        // GET api/<controller>
        public IHttpActionResult Get()
        {
            try
            {
                const string cacheKey = "GetAllVehicles";

                _cacheService.TryGetValue(cacheKey, out List<Vehicle> vehicles);

                if (vehicles == null || vehicles.Count == 0)
                {
                    vehicles = new List<Vehicle>();
                    vehicles = _vehicleData.GetVehicles();

                    _cacheService.Set(cacheKey, vehicles, null);
                }
                
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }           
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Invalid Vehicle ID.");
                }

                string cacheKey = "GetVehicleId=" + id;

                _cacheService.TryGetValue(cacheKey, out Vehicle vehicle);

                if (vehicle == null)
                {
                    vehicle = new Vehicle();


                    vehicle = _vehicleData.GetVehicle(id);

                    if (vehicle.VehicleId > 0)
                    {
                        _cacheService.Set(cacheKey, vehicle, null);
                    }
                }

                return Ok(vehicle);

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST api/<controller>
        public IHttpActionResult Post([FromBody] Vehicle vehicle)
        {
            if ((vehicle.Make == String.Empty || vehicle.Make == null)
                || (vehicle.Trim == String.Empty || vehicle.Trim == null)
                || (vehicle.Model == String.Empty || vehicle.Model == null)
                || vehicle.ModelYear <= 1930)

            {
                return BadRequest("Make, Model, and Trim must be filled out, Model Year must be greater than 1930");
            }

            try
            {
                vehicle = _vehicleData.InsertVehicle(vehicle);
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/<controller>
        public IHttpActionResult Put([FromBody] Vehicle vehicle)
        {
            if (vehicle.VehicleId <= 0)
            {
                return BadRequest("Invalid Vehicle ID.");
            }

            string cacheKey = "GetVehicleId=" + vehicle.VehicleId;

            try
            {
                string updateQuery = "UPDATE Vehicle SET ";
                    
                var parameters = new List<SqlParameter>();

                parameters.Add(new SqlParameter("@VehicleId", vehicle.VehicleId));

                if (!string.IsNullOrEmpty(vehicle.Make))
                {
                    updateQuery += "Make = @Make, ";
                    parameters.Add(new SqlParameter("@Make", vehicle.Make));
                }
                if (!string.IsNullOrEmpty(vehicle.Model))
                {
                    updateQuery += "Model = @Model, ";
                    parameters.Add(new SqlParameter("@Model", vehicle.Model));
                }
                if (!string.IsNullOrEmpty(vehicle.Trim))
                {
                    updateQuery += "Trim = @Trim, ";
                    parameters.Add(new SqlParameter("@Trim", vehicle.Trim));
                }
                if (vehicle.ModelYear > 1930)
                {
                    updateQuery += "ModelYear = @ModelYear, ";
                    parameters.Add(new SqlParameter("@ModelYear", vehicle.ModelYear));
                }

                updateQuery = updateQuery.TrimEnd(',', ' ') + " WHERE VehicleId = @VehicleId";
                    
                Vehicle updatedVehicle = _vehicleData.UpdateVehicle(parameters, updateQuery, vehicle);

                _cacheService.Set(cacheKey, updatedVehicle, null);
                _cacheService.Remove("GetAllVehicles");
                return Ok(updatedVehicle);                    
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE api/<controller>/5
        public IHttpActionResult Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Vehicle ID.");
            }

            try
            {
                int rowsAffected = _vehicleData.DeleteVehicle(id);
       
                if (rowsAffected > 0)
                {
                    _cacheService.Remove("GetAllVehicles");
                    _cacheService.Remove("GetVehicleId=" + id);
                    return Ok("Vehicle and associated fitments deleted!");
                }
                else
                {
                    return NotFound();
                }               
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}