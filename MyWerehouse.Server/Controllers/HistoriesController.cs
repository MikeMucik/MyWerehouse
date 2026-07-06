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
	[Route("api/histories")]
	public class HistoriesController : ControllerBase
	{
		private readonly IMediator _mediator;
		public HistoriesController(IMediator mediator)
		{
			_mediator = mediator;
		}
		
		[HttpGet("pallets/{palletNumber}")]
		public async Task<IActionResult> GetPalletHistory(string palletNumber)
		{
			var query = new GetPalletHistoryQuery
			{
				PalletNumber = palletNumber
			};
			return (await _mediator.Send(query)).ToActionResult();
		}
				
	}
}

