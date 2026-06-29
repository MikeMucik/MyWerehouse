using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	
	[ApiController]
	[Route("api/client")]
	public class ClientController : ControllerBase
	{
		private readonly IClientService _clientService;
		public ClientController(IClientService clientService)
		{
			_clientService = clientService;
		}
		[HttpPost("add")]
		public async Task<IActionResult> Add(AddClientDTO clientDTO)
		{
			var result = await _clientService.AddClientAsync(clientDTO);
			return result.ToActionResult();
		}
		[HttpPost("{id}delete")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _clientService.DeleteClientAsync(id);
			return result.ToActionResult();
		}
		[HttpPost("{id}update")]
		public async Task<IActionResult> Update(int id,UpdateClientDTO clientDTO)
		{
			var result = await _clientService.UpdateClientAsync(id,clientDTO);
			return result.ToActionResult();
		}
		[HttpGet("{id}fullInfo")]
		public async Task<IActionResult> GetFullInfo(int id)
		{
			var result = await _clientService.GetClientToEditAsync(id);
			return result.ToActionResult();
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAll(int page, int size, CancellationToken ct)
		{
			var result = await _clientService.GetAllClientsAsync(page, size, ct);
			return result.ToActionResult();
		}
		[HttpGet("byFilter")]
		public async Task<IActionResult> GetByFiltr(int page, int size,[FromQuery] ClientSearchFilter filter, CancellationToken ct)//można zamienić na [FromQuery] +DTO
		{
			var result = await _clientService.GetClientsByFilterAsync(page, size, filter, ct);
			return result.ToActionResult();
		}
	}
}
