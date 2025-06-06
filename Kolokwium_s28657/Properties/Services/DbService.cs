namespace Kolokwium_s28657.Properties;

public class DbService : IDbService
{
    private readonly IConfiguration _config;
    public DbService(IConfiguration config) => _config = config;
    public SqlConnection GetConnection() => new SqlConnection(_config.GetConnectionString("Default"));
}