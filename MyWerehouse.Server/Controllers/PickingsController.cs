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
		public async Task<IActionResult> ClosePallet(ClosePickingPalletCommand command)
			=> (await _mediator.Send(command)).ToActionResult();
		
		[HttpPost("planned")]
		public async Task<IActionResult> PlannedPicking(DoPlannedPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		[HttpPost("emergency")]
		public async Task<IActionResult> EmergencyPicking(ExecuteEmergencyPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		[HttpPost("manual")]
		public async Task<IActionResult> ManualPicking(ExecuteHandPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Zakończ planowane/korygowane zadania kompletacyjne, stwórz ręczne
		[HttpPost("switch-to-manual")]
		public async Task<IActionResult> SwitchToHandPicking(FinishPlannedPickingPrepareToHandPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();
	
		[HttpGet("planned-tasks")]
		public async Task<IActionResult> ShowPlanned([FromQuery]ShowTaskToDoQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		[HttpGet("emergency-options")]
		public async Task<IActionResult> GetEmergencyOptions	([FromQuery]PrepareEmergencyPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();


		[HttpGet("issues")]
		public async Task<IActionResult> GetList([FromQuery]GetListIssueToPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		[HttpGet("issues-tree")]
		public async Task<IActionResult> GetTree([FromQuery]GetListToPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		[HttpGet("forklift-pallets")]
		public async Task<IActionResult> GetListToPicking(DateOnly dateStart, DateOnly dateEnd, int pageNumber, int pageSize)
			=> (await _mediator.Send(new GetListPickingPalletQuery(dateStart, dateEnd, pageNumber,pageSize))).ToActionResult();
	}
}
