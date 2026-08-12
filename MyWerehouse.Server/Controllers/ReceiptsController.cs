using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Application.Receipts.Commands.CancelReceipt;
using MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt;
using MyWerehouse.Application.Receipts.Commands.CreateReceipt;
using MyWerehouse.Application.Receipts.Commands.DeleteDraftReceipt;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt;
using MyWerehouse.Application.Receipts.Queries.GetReceiptById;
using MyWerehouse.Application.Receipts.Queries.GetReceiptsByFilter;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/receipts")]
	public class ReceiptsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public ReceiptsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		//Stworzenie pustego przyjęcia
		[HttpPost]
		public async Task<IActionResult> Create(CreateReceiptPlanCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		//Przyjęcie palety dla Receipt
		[HttpPost("{id:guid}/pallets")]
		public async Task<IActionResult> CreatePalletForReceipt(Guid id, CreatePalletReceiptDTO dto, CancellationToken ct)
			=> (await _mediator.Send(new AddPalletToReceiptCommand(id, dto), ct))
			.ToActionResult();

		//Aktualizacja przyjęcia, poprawa palet -> Post
		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, UpdateReceiptDTO dto, CancellationToken ct)
			=> (await _mediator.Send(new UpdateReceiptCommand(id, dto), ct))
			.ToActionResult();

		//Anulowanie przyjęcia, kasacja - nie ma wpływu na stan -> zatwierdzone nie można cofnąć

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new DeleteDraftReceiptCommand(id, userId), ct))
			.ToActionResult();

		[HttpPost("{id:guid}/cancel")]
		public async Task<IActionResult> Cancel(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new CancelReceiptCommand(id, userId), ct))
			.ToActionResult();

		//Zatwierdzenie skończenia rozładunku - magazyn
		[HttpPost("{id:guid}/complete-unloading")]
		public async Task<IActionResult> ConfirmEndReceipt(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new CompletePhysicalReceiptCommand(id, userId), ct))
			.ToActionResult();

		//Zatwierdzenie rozładunku biuro - zmiana stanu magazynowego, palety w obiekgu
		[HttpPost("{id:guid}/finalize")]
		public async Task<IActionResult> FinalizeReceipt(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new VerifyAndFinalizeReceiptCommand(id, userId), ct))
			.ToActionResult();

		//Pobranie przyjęcia np do edycji
		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetReceiptByIdQuery(id), ct))
			.ToActionResult();

		//Pobranie przyjęć
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] GetReceiptsByFilterQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();
	}

}
