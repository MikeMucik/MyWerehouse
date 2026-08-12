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
			List<Guid> palletsIds, int? rampNumber, CancellationToken ct)
			=> (await _mediator.Send(new ExecuteReversePickingCommand(id, strategy,
				pickingPalletId, userId, palletsIds, rampNumber), ct))
			.ToActionResult();

		[HttpGet]
		public async Task<IActionResult> Tasks ([FromQuery] GetListReversePickingToDoQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> TaskOptions(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetReversePickingToDoQuery(id), ct))
			.ToActionResult();

		[HttpGet("available-pallets")]
		public async Task<IActionResult> PalletsForReservePicking([FromQuery] ListPalletsForForkLifterReservePickingQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct))
			.ToActionResult();
	}
}
