using Kolokwium_s28657.Properties;
using Microsoft.AspNetCore.Mvc;
using ControllerBase = Kolokwium_s28657.Properties.ControllerBase;

public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }
    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient(int clientId)
    {
        var result = await _clientService.GetClientWithRentals(clientId);
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpPost]
    public async Task<IActionResult> CreateClientWithRental([FromBody] CreateClientRentalRequest request)
    {
        if (request.DateTo <= request.DateFrom)
            return BadRequest("Invalid dates.");

        var id = await _clientService.CreateClientWithRentalAsync(request);
        return Created($"api/clients/{id}", null);
    }
}