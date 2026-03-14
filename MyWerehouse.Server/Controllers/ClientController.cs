using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;

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
		[HttpPost]
		public async Task<IActionResult> Add(AddClientDTO clientDTO)
		{
			var result = await _clientService.AddClientAsync(clientDTO);
			return Ok(result);
		}
		[HttpPost("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _clientService.DeleteClientAsync(id);
			return Ok(result);
		}
		[HttpPost("update")]
		public async Task<IActionResult> Update(UpdateClientDTO clientDTO)
		{
			var result = await _clientService.UpdateClientAsync(clientDTO);
			return Ok(result);
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _clientService.GetClientToEditAsync(id);
			return Ok(result);
		}
		[HttpGet]
		public async Task<IActionResult> GetAll(int page, int size)//można zamienić na [FromQuery] +DTO
		{
			var result = await _clientService.GetAllClientsAsync(page, size);
			return Ok(result);
		}
		[HttpGet("byFilter")]
		public async Task<IActionResult> GetByFiltr(int page, int size, ClientSearchFilter filter)//można zamienić na [FromQuery] +DTO
		{
			var result = await _clientService.GetClientsByFilterAsync(page, size, filter);
			return Ok(result);
		}
	}
}
