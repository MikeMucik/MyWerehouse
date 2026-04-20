using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.Commands.DeleteIssue;
using MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted;
using MyWerehouse.Application.Issues.Commands.UpdateIssue;
using MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr;
using MyWerehouse.Application.Issues.Queries.LoadingIssueList;
using MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/issues")]
	public class IssuesController : ControllerBase
	{
		private readonly IMediator _mediator;

		public IssuesController(IMediator mediator)
		{
			_mediator = mediator;
		}

		//Stworzenie zlecenia wydania
		[HttpPost("add")]
		public async Task<IActionResult> Create(CreateNewIssueCommand command)
		{
			var result = await _mediator.Send(command);
			return result.ToActionResult();
		}

		//Do edycji lub przejrzenia zlecenia
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var result = await _mediator.Send(new GetIssueByIdQuery(id));
			return Ok(result);
		}

		// Update - wiele rozwiązań więc POST
		[HttpPost("update")]
		public async Task<IActionResult> Update(UpdateIssueNewCommand command)
		{
			var result = await _mediator.Send(command);
			return result.ToActionResult();
		}

		//Przypadek szczególny, gdy zlecenie "świeże"
		[HttpDelete("delete")]
		public async Task<IActionResult> Delete(DeleteIssueCommand command)
			=> (await  _mediator.Send(command)).ToActionResult();

		//Zmiana statusu zlecenia i inne akcje więc POST - anulowanie
		[HttpPost("cancel")]
		public async Task<IActionResult> Cancel(CancelIssueCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Zamiana palet dla Issue (np problem fizyczny na magazynie zablokowany dostęp)
		[HttpPost("changePallet")]
		public async Task<IActionResult> PalletReplacement(ChangePalletInIssueCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Zatwierdzenie magazynowe że załadunek skończony
		[HttpPost("confirmLoading")]
		public async Task<IActionResult> ConfirmEndLoading(CompletedLoadIssueCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Zatwierdzenie biurowe koniec załadunku gdy załadunek przerwany(np brak miejsca na aucie)
		[HttpPost("EndDuringLoading")]
		public async Task<IActionResult> BreakLoadingConfirmEndLoading(FinishIssueNotCompletedCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Weryfikacja(biuro) po załadunku - aktualizacja stanów magazynowych
		[HttpPost("confirmAfter")]
		public async Task<IActionResult> VerificationAfterLoad(VerifyIssueAfterLoadingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Weryfikacja załadunku przed załadunkiem - porównania co zamówino vs co przygotowano
		[HttpPost("confirmBefore")]
		public async Task<IActionResult> VerificationBeforeLoad(VerifyIssueToLoadCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Listy 

		//Lista dla Issue ile jakiego towaru
		[HttpGet("{id}/byProducts")]
		public async Task<IActionResult> ListProductsForIssue(Guid id)
			=> (await _mediator.Send(new GetIssueProductSummaryByIdQuery(id))).ToActionResult();

		//Lista dla Issue według filtra
		[HttpGet("IssuesByFiltr")]
		public async Task<IActionResult> ListIssuesByFiltr(GetIssuesByFiltrQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Lista dla Issue ile jakiego towaru
		[HttpGet("{id}/loadingList")]
		public async Task<IActionResult> ListForLoad(Guid id)
			=> (await _mediator.Send(new LoadingIssueListQuery(id))).ToActionResult();

		//Lista palet do "zdjęcia" dla operatora wózka
		[HttpGet("{id}/forOperator")]
		public async Task<IActionResult> ListPalletsForTheForklift(Guid id)
			 => (await _mediator.Send(new PalletsToTakeOffListQuery(id))).ToActionResult();	
	}
}
