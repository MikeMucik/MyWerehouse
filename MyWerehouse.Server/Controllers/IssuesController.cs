using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Application.Issues.Commands.ConfirmIssueAfterLoading;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.Commands.DeleteIssue;
using MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted;
using MyWerehouse.Application.Issues.Commands.ModifyIssue;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFilter;
using MyWerehouse.Application.Issues.Queries.IssueProductsSummary;
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

		[HttpPost]
		public async Task<IActionResult> Create(CreateIssueCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct))
			.ToActionResult();

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> Get(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetIssueByIdQuery(id), ct))
			.ToActionResult();

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id,ModifyIssueDTO dto, DateOnly dateToSend, CancellationToken ct)
			=> (await _mediator.Send(new ModifyIssueCommand(id, dto, dateToSend), ct)).ToActionResult();


		//Przypadek szczególny, gdy zlecenie "świeże"
		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, string userId, CancellationToken ct)
			=> (await  _mediator.Send(new DeleteIssueCommand(id, userId), ct))
			.ToActionResult();

		//Zmiana statusu zlecenia i inne akcje więc POST - anulowanie
		[HttpPost("{id:guid}/cancel")]
		public async Task<IActionResult> Cancel(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new CancelIssueCommand(id, userId), ct))
			.ToActionResult();

		//Zamiana palet dla Issue (np problem fizyczny na magazynie zablokowany dostęp)
		[HttpPost("{id:guid}/change-pallet")]
		public async Task<IActionResult> PalletReplacement(Guid id, Guid oldPalletId, Guid newPalletId, string userId, CancellationToken ct)
			=> (await _mediator.Send(new ChangePalletInIssueCommand(id, oldPalletId, newPalletId, userId), ct))
			.ToActionResult();

		[HttpPost("{id:guid}/confirm-loading")]
		public async Task<IActionResult> ConfirmEndLoading(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new CompletedLoadIssueCommand(id, userId), ct))
			.ToActionResult();

		//Zatwierdzenie biurowe koniec załadunku gdy załadunek przerwany(np brak miejsca na aucie)
		[HttpPost("{id:guid}/finish-loading")]
		public async Task<IActionResult> BreakLoadingConfirmEndLoading(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new FinishIssueNotCompletedCommand(id, userId), ct))
			.ToActionResult();

		//Weryfikacja(biuro) po załadunku - aktualizacja stanów magazynowych
		[HttpPost("{id:guid}/verify-after-loading")]
		public async Task<IActionResult> VerificationAfterLoad(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new ConfirmIssueAfterLoadingCommand(id, userId), ct)).ToActionResult();

		//Weryfikacja załadunku przed załadunkiem - porównania co zamówino vs co przygotowano
		[HttpPost("{id:guid}/verify-before-loading")]
		public async Task<IActionResult> VerificationBeforeLoad(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new VerifyIssueToLoadCommand(id, userId), ct)).ToActionResult();

		//Listy

		//Lista dla Issue ile jakiego towaru
		[HttpGet("{id:guid}/products")]
		public async Task<IActionResult> ListProductsForIssue(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new IssueProductsSummaryQuery(id), ct)).ToActionResult();

		//Lista dla Issue według filtra
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery]GetIssuesByFilterQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();

		//Lista dla Issue ile jakiego towaru
		[HttpGet("{id:guid}/loading-list")]
		public async Task<IActionResult> ListForLoad(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new LoadingIssueListQuery(id), ct)).ToActionResult();

		//Lista palet do "zdjęcia" dla operatora wózka
		[HttpGet("{id:guid}/operator-pallets")]
		public async Task<IActionResult> ListPalletsForTheForklift(Guid id, int pageNumber, int pageSize, CancellationToken ct)
			 => (await _mediator.Send(new PalletsToTakeOffListQuery(id, pageNumber, pageSize), ct)).ToActionResult();
	}
}
