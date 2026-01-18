using System.Data;
using Npgsql;

namespace PortfolioPro.Data;

public class DbConnectionFactory(IConfiguration configuration)
{
    // Retrieves connection string from appsettings.json; throws error if missing.
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Returns a new PostgreSQL connection. 
    // Repositories use this to run Dapper commands.
    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}