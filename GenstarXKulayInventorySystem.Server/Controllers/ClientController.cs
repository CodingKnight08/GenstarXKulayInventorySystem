using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet("all/{branch}")]
    public async Task<ActionResult<List<ClientDto>>> GetAllClient([FromRoute] BranchOption branch)
    {
        var clients = await _clientService.GetAllClientsAsync(branch);
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
    {
        var client = await _clientService.GetClientById(id);
        if (client == null)
            return NotFound();
        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<int?>> Create(ClientDto dto)
    {
        var result = await _clientService.AddClientAsync(dto);
        if (result == null)
            return BadRequest("Client already exists.");
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ClientDto dto)
    {
        if (id != dto.Id)
            return BadRequest("Client ID mismatch.");
        var updatedClient = await _clientService.UpdateClientAsync(dto);
        if (!updatedClient)
            return NotFound("Client not found or already exists.");
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deletedClient = await _clientService.DeleteClientAsync(id);
        if (!deletedClient)
            return NotFound("Client not found.");
        return Ok();
    }
}
