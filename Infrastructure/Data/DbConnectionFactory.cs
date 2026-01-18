using System.Data;
using Npgsql;

namespace PortfolioPro.Data;
/**This facilitates the database connection. 
because there are single databases in your area.**/
public class DbConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}