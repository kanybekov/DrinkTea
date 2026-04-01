using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DrinkTea.DataAccess;

public class DbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException();
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
