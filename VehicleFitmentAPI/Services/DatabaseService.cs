using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace VehicleFitmentAPI.Services
{
    public interface IDatabaseService
    {
        SqlConnection GetConnectionString();
    }
    
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlConnection GetConnectionString()
        {
            return new SqlConnection(_connectionString);
        }
    }
}