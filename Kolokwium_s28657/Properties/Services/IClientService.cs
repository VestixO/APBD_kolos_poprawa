namespace Kolokwium_s28657.Properties;

public interface IClientService
{
    Task<ClientRentalDto?> GetClientWithRentals(int clientId);
    Task<int> CreateClientWithRentalAsync(CreateClientRentalRequest request);
    
}