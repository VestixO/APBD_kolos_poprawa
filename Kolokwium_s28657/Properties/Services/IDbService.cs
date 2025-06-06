namespace Kolokwium_s28657.Properties;

public interface IDbService
{
    SqlConnection GetConnection();
}