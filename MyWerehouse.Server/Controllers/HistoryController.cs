using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Histories.Queries.GetPalletHistoryQuery;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Picking.Queries.GetListPickingPallet;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/history")]
	public class HistoryController : ControllerBase
	{
		private readonly IMediator _mediator;
		public HistoryController(IMediator mediator)
		{
			_mediator = mediator;
		}
		
		[HttpGet("FindHistoryRecordForPalletByFilter")]
		public async Task<IActionResult> FindHistoryRecordForPalletByFilter([FromQuery] GetPalletHistoryQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}

