using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Picking.Commands.ClosePickingPallet;
using MyWerehouse.Application.Picking.Commands.DoPlannedPicking;
using MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking;
using MyWerehouse.Application.Picking.Commands.ExecuteHandPicking;
using MyWerehouse.Application.Picking.Commands.FinishPlannedPickingPrepareToHandPicking;
using MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree;
using MyWerehouse.Application.Picking.Queries.GetListPickingPallet;
using MyWerehouse.Application.Picking.Queries.GetListToPickingFlat;
using MyWerehouse.Application.Picking.Queries.PrepareCorrectedPicking;
using MyWerehouse.Application.Picking.Queries.ShowTaskToDo;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{

	[ApiController]
	[Route("api/pickings")]
	public class PickingsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public PickingsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost("close")]
		public async Task<IActionResult> ClosePallet(ClosePickingPalletCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		[HttpPost("planned")]
		public async Task<IActionResult> PlannedPicking(DoPlannedPickingCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		[HttpPost("emergency")]
		public async Task<IActionResult> EmergencyPicking(ExecuteEmergencyPickingCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		[HttpPost("manual")]
		public async Task<IActionResult> ManualPicking(ExecuteHandPickingCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		//Zakończ planowane/korygowane zadania kompletacyjne, stwórz ręczne
		[HttpPost("switch-to-manual")]
		public async Task<IActionResult> SwitchToHandPicking(FinishPlannedPickingPrepareToHandPickingCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		[HttpGet("planned-tasks")]
		public async Task<IActionResult> ShowPlanned([FromQuery]ShowTaskToDoQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();

		[HttpGet("emergency-options")]
		public async Task<IActionResult> GetEmergencyOptions	([FromQuery]PrepareEmergencyPickingQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();


		[HttpGet("issues")]
		public async Task<IActionResult> GetList([FromQuery]GetListIssueToPickingQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();

		[HttpGet("issues-tree")]
		public async Task<IActionResult> GetTree([FromQuery]GetListToPickingQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();

		[HttpGet("forklift-pallets")]
		public async Task<IActionResult> GetListToPicking(DateOnly dateStart, DateOnly dateEnd, int pageNumber, int pageSize, CancellationToken ct)
			=> (await _mediator.Send(new GetListPickingPalletQuery(dateStart, dateEnd, pageNumber,pageSize), ct)).ToActionResult();
	}
}
