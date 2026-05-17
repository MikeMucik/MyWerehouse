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

		//Zamknięcie palety kompletacyjnej
		[HttpPost("close")]
		public async Task<IActionResult> ClosePallet(ClosePickingPalletCommand command)
			=> (await _mediator.Send(command)).ToActionResult();
		
		//Dodaj towar do palety kompletacyjnej

		//Wykonaj planowane zadanie kompletacyjne
		[HttpPost("planned")]
		public async Task<IActionResult> PlannedPicking(DoPlannedPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Wykonaj skorygowane zadanie kompletacyjne
		[HttpPost("corrected")]
		public async Task<IActionResult> CorrectedPicking(ExecuteEmergencyPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Wykonaj awaryjne zadanie kompletacyjne
		[HttpPost("emergency")]
		public async Task<IActionResult> HandPicking(ExecuteHandPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Zakończ planowane/korygowane zadania kompletacyjne, stwórz awaryjne
		[HttpPost("swithHand")]
		public async Task<IActionResult> SwitchToHandPicking(FinishPlannedPickingPrepareToHandPickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Pokaż zadania do wykonania

		//Podaj listę zadań dla palety - kompletacja planowana
		[HttpGet("plannedList")]
		public async Task<IActionResult> ShowPlanned([FromQuery]ShowTaskToDoQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Podaj listę zadań dla palety - kompletacja skorygowana
		[HttpGet("correctedList")]
		public async Task<IActionResult> ShowCorrected([FromQuery]PrepareCorrectedPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Lista poglądowa klient -> zamówienie -> produkt -> ilośc		

		//Lista
		[HttpGet("List")]
		public async Task<IActionResult> GetList([FromQuery]GetListIssueToPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Drzewo
		[HttpGet("Tree")]
		public async Task<IActionResult> GetTree([FromQuery]GetListToPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Lista palet dla wózkowego- palety do przekazania do pickingu
		[HttpPost("forkliftList")]
		public async Task<IActionResult> GetListToPicking([FromQuery]GetListPickingPalletQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}
