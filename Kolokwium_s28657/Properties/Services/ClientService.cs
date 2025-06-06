namespace Kolokwium_s28657.Properties;

public class ClientService : IClientService
{
    private readonly IDbService _db;

    public ClientService(IDbService db)
    {
        _db = db;
    }

    public async Task<ClientRentalDto?> GetClientWithRentals(int clientId)
    {
        using var con = _db.GetConnection();
        using var cmd = con.CreateCommand();
        cmd.CommandText = @"
            SELECT c.ID, c.FirstName, c.LastName, c.Address,
                   cr.DateFrom, cr.DateTo, cr.TotalPrice,
                   ca.VIN, co.Name AS Color, mo.Name AS Model
            FROM clients c
            LEFT JOIN car_rentals cr ON cr.ClientID = c.ID
            LEFT JOIN cars ca ON cr.CarID = ca.ID
            LEFT JOIN colors co ON ca.ColorID = co.ID
            LEFT JOIN models mo ON ca.ModelID = mo.ID
            WHERE c.ID = @id";
        cmd.Parameters.AddWithValue("@id", clientId);

        await con.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        ClientRentalDto? client = null;
        while (await reader.ReadAsync())
        {
            if (client == null)
            {
                client = new ClientRentalDto
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Address = reader.GetString(3),
                    Rentals = new()
                };
            }

            if (!reader.IsDBNull(4))
            {
                client.Rentals.Add(new RentalDto
                {
                    DateFrom = reader.GetDateTime(4),
                    DateTo = reader.GetDateTime(5),
                    TotalPrice = reader.GetInt32(6),
                    Vin = reader.GetString(7),
                    Color = reader.GetString(8),
                    Model = reader.GetString(9)
                });
            }
        }
        return client;
    }

    public async Task<int> CreateClientWithRentalAsync(CreateClientRentalRequest req)
    {
        using var con = _db.GetConnection();
        await con.OpenAsync();
        using var tran = con.BeginTransaction();
        
        using (var checkCmd = con.CreateCommand())
        {
            checkCmd.Transaction = tran;
            checkCmd.CommandText = "SELECT PricePerDay FROM cars WHERE ID = @carId";
            checkCmd.Parameters.AddWithValue("@carId", req.CarId);
            var priceObj = await checkCmd.ExecuteScalarAsync();
            if (priceObj == null)
                throw new Exception("Car not found");

            var pricePerDay = Convert.ToInt32(priceObj);
            int days = (req.DateTo - req.DateFrom).Days;
            int total = pricePerDay * days;
            
            int clientId;
            using (var insertClient = con.CreateCommand())
            {
                insertClient.Transaction = tran;
                insertClient.CommandText = @"INSERT INTO clients (FirstName, LastName, Address)
                                            VALUES (@fn, @ln, @addr); SELECT SCOPE_IDENTITY();";
                insertClient.Parameters.AddWithValue("@fn", req.Client.FirstName);
                insertClient.Parameters.AddWithValue("@ln", req.Client.LastName);
                insertClient.Parameters.AddWithValue("@addr", req.Client.Address);
                clientId = Convert.ToInt32(await insertClient.ExecuteScalarAsync());
            }

            using var insertRental = con.CreateCommand();
            insertRental.Transaction = tran;
            insertRental.CommandText = @"INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice)
                                         VALUES (@cid, @carid, @df, @dt, @total)";
            insertRental.Parameters.AddWithValue("@cid", clientId);
            insertRental.Parameters.AddWithValue("@carid", req.CarId);
            insertRental.Parameters.AddWithValue("@df", req.DateFrom);
            insertRental.Parameters.AddWithValue("@dt", req.DateTo);
            insertRental.Parameters.AddWithValue("@total", total);
            await insertRental.ExecuteNonQueryAsync();

            await tran.CommitAsync();
            return clientId;
        }
    }
}