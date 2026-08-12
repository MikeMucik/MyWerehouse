using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ClientModels;
using MyWerehouse.Domain.Clients.Filters;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{

	[ApiController]
	[Route("api/clients")]
	public class ClientsController : ControllerBase
	{
		private readonly IClientService _clientService;
		public ClientsController(IClientService clientService)
		{
			_clientService = clientService;
		}
		[HttpPost]
		public async Task<IActionResult> Create(AddClientDTO clientDto, CancellationToken ct)
			=> (await _clientService.AddClientAsync(clientDto, ct))
			.ToActionResult();

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id, CancellationToken ct)
			=> (await _clientService.DeleteClientAsync(id, ct))
			.ToActionResult();

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, UpdateClientDTO clientDto, CancellationToken ct)
			=> (await _clientService.UpdateClientAsync(id, clientDto, ct))
			.ToActionResult();

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetById(int id, CancellationToken ct)
			=> (await _clientService.GetClientByIdAsync(id, ct))
			.ToActionResult();

		[HttpGet]
		public async Task<IActionResult> GetAll(
			[FromQuery]	int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			CancellationToken ct = default)
			=> (await _clientService.GetAllClientsAsync(pageNumber, pageSize, ct))
			.ToActionResult();

		[HttpGet("search")]
		public async Task<IActionResult> Search(
			[FromQuery] ClientSearchFilter filter,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			CancellationToken ct = default)
			=> (await _clientService.GetClientsByFilterAsync(pageNumber, pageSize, filter, ct))
			.ToActionResult();
	}
}
