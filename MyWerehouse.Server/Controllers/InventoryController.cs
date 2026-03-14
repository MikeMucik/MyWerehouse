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
		private readonly IInventoryService _inventoryService;
		private readonly IMediator _mediator;
		public InventoryController(IInventoryService inventoryService,
			IMediator mediator)
		{
			_inventoryService = inventoryService;
			_mediator = mediator;
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var resut = await _mediator.Send(new GetInventoryQuery(id));
			return Ok(resut);
		}
		//[HttpGet]
		//public async Task<IActionResult> GetByProduct(int id, DateOnly date)
		//{
		//	var resut = await _inventoryService.GetProductCountAsync(id, date);
		//	return Ok(resut);
		//}


		//[HttpPost]
		//public async Task<IActionResult> Get(int id, int quantity)
		//{
		//	//var resut =
		//	await _inventoryService.ChangeProductQuantityAsync(id, quantity);
		//	return Ok();
		//		//(resut);
		//}
	}
}
