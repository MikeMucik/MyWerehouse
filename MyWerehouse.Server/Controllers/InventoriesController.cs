using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Inventories.Queries.GetInventories;
using MyWerehouse.Application.Inventories.Queries.GetInventory;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/inventories")]
	public class InventoriesController(IMediator mediator) : ControllerBase
	{
		private readonly IMediator _mediator = mediator;

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> Get(Guid id, CancellationToken ct)
		{
			var result = await _mediator.Send(new GetInventoryQuery(id), ct);
			return result.ToActionResult();
		}

		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] int page = 1,
			[FromQuery] int size = 10,
			CancellationToken ct = default)
		{
			var query = new GetInventoriesQuery(PageNumber : page, PageSize: size);
			var result = await _mediator.Send(query, ct);
			return result.	ToActionResult();
		}			 


	}
}
