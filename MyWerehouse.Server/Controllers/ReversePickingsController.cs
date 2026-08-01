using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking;
using MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.ListPalletsForForkLifterReservePicking;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	
	[ApiController]
	[Route("api/reverse-pickings")]
	public class ReversePickingsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public ReversePickingsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("{id:guid}")]
		public async Task<IActionResult> Execute(
			Guid id, ReversePickingStrategy strategy,
			Guid pickingPalletId, string userId,
			List<Guid> palletsIds, int? rampNumber )
			=> (await _mediator.Send(new ExecuteReversePickingCommand(id, strategy,
				pickingPalletId, userId, palletsIds, rampNumber)))
			.ToActionResult();

		[HttpGet]
		public async Task<IActionResult> Tasks ([FromQuery] GetListReversePickingToDoQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> TaskOptions(Guid id)
			=> (await _mediator.Send(new GetReversePickingToDoQuery(id)))
			.ToActionResult();

		[HttpGet("available-pallets")]
		public async Task<IActionResult> PalletsForReservePicking([FromQuery] ListPalletsForForkLifterReservePickingQuery query)
			=> (await _mediator.Send(query))
			.ToActionResult();
	}
}
