using System.Data.SqlClient;

namespace VehicleFitmentAPI.Services
{
    
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