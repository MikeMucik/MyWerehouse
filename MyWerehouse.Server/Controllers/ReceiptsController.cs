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

		//Create an empty receipt
		[HttpPost]
		public async Task<IActionResult> Create(CreateReceiptPlanCommand command, CancellationToken ct)
			=> (await _mediator.Send(command, ct)).ToActionResult();

		//Receive a pallet for a receipt
		[HttpPost("{id:guid}/pallets")]
		public async Task<IActionResult> CreatePalletForReceipt(Guid id, CreatePalletReceiptDTO dto, CancellationToken ct)
			=> (await _mediator.Send(new AddPalletToReceiptCommand(id, dto), ct))
			.ToActionResult();

		//Update the receipt and correct its pallets -> POST
		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, UpdateReceiptDTO dto, CancellationToken ct)
			=> (await _mediator.Send(new UpdateReceiptCommand(id, dto), ct))
			.ToActionResult();

		//Cancel/delete the receipt without affecting stock; a verified receipt cannot be reversed

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new DeleteDraftReceiptCommand(id, userId), ct))
			.ToActionResult();

		[HttpPost("{id:guid}/cancel")]
		public async Task<IActionResult> Cancel(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new CancelReceiptCommand(id, userId), ct))
			.ToActionResult();

		//Warehouse confirmation that unloading is complete
		[HttpPost("{id:guid}/complete-unloading")]
		public async Task<IActionResult> ConfirmEndReceipt(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new CompletePhysicalReceiptCommand(id, userId), ct))
			.ToActionResult();

		//Office verification of unloading: update stock and make pallets available
		[HttpPost("{id:guid}/finalize")]
		public async Task<IActionResult> FinalizeReceipt(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new VerifyAndFinalizeReceiptCommand(id, userId), ct))
			.ToActionResult();

		//Retrieve a receipt, e.g. for editing
		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetReceiptByIdQuery(id), ct))
			.ToActionResult();

		//Retrieve receipts
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] GetReceiptsByFilterQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();
	}

}
