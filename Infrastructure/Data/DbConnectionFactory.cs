using System.Data;
using Npgsql;

namespace PortfolioPro.Data;
/**This facilitates the database connection. 
because there are single databases in your area.**/

/// <summary>
/// Provides a centralized factory for creating and managing postgreSQL database
/// connections. This ensures consistent connection string usage and simplifies dependency
/// injection for repositories.
/// </summary>
public class DbConnectionFactory(IConfiguration configuration)
{
    // Holds the validated connection string retrieved from appsettings.json
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    /// <summary>
    /// Creates and returns a new, uninitialized connection the PostgresSQL database.
    /// </summary>
    /// <returns>An <see cref="IDBConnection"/> instance specifically using the NpgSQl provider. </returns>
    /// <remarks>
    /// The caller is responsible for opening and disposing of the connection, 
    /// typically handled within a 'using' block in the repository layer.
    /// </remarks>
    public IDbConnection Create() => new NpgsqlConnection(_connectionString);
}