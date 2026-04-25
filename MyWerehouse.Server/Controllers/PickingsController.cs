using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.PickingPallets.Commands.ClosePickingPallet;
using MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking;
using MyWerehouse.Application.PickingPallets.Commands.ExecuteEmergencyPicking;
using MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking;
using MyWerehouse.Application.PickingPallets.Commands.FinishPlannedPickingPrepareToHandPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListPickingPallet;
using MyWerehouse.Application.PickingPallets.Queries.GetListToPicking;
using MyWerehouse.Application.PickingPallets.Queries.PrepareCorrectedPicking;
using MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo;
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
		public async Task<IActionResult> ShowPlanned(ShowTaskToDoQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Podaj listę zadań dla palety - kompletacja skorygowana
		[HttpGet("correctedList")]
		public async Task<IActionResult> ShowCorrected(PrepareCorrectedPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Lista poglądowa klient -> zamówienie -> produkt -> ilośc		

		//Lista
		[HttpGet("List")]
		public async Task<IActionResult> GetList(GetListIssueToPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Drzewo
		[HttpGet("Tree")]
		public async Task<IActionResult> GetTree(GetListToPickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Lista palet dla wózkowego- palety do przekazania do pickingu
		[HttpGet("forkliftList")]
		public async Task<IActionResult> GetListToPicking(GetListPickingPalletQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}
