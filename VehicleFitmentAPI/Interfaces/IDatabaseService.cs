using System.Collections.Generic;
using System.Data.SqlClient;
using VehicleFitmentAPI.Models;

public interface IDatabaseService
{
    SqlConnection GetConnectionString();
}