using MediatR;
using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> Get(Guid id)
		{
			var result = await _mediator.Send(new GetInventoryQuery(id));
			return result.ToActionResult();
		}		
	}
}
