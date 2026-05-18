using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.Queries.GetInventory;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/inventory")]
	public class InventoryController : ControllerBase
	{		
		private readonly IMediator _mediator;
		public InventoryController(IMediator mediator)
		{
			_mediator = mediator;
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var result = await _mediator.Send(new GetInventoryQuery(id));
			return Ok(result);
		}
		
	}
}
