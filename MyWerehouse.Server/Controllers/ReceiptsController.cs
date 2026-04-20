using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt;
using MyWerehouse.Application.Receipts.Commands.CreateReceipt;
using MyWerehouse.Application.Receipts.Commands.DeleteReceipt;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt;
using MyWerehouse.Application.Receipts.Queries.GetReceipt;
using MyWerehouse.Application.Receipts.Queries.GetReceipts;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{	
	[ApiController]
	[Route("api/recipe")]
	public class ReceiptsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public ReceiptsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		//Stworzenie pustego przyjęcia
		[HttpPost("new")]
		public async Task<IActionResult> Create(CreateReceiptPlanCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Przyjęcie palety dla Receipt
		[HttpPost("addPallet")]
		public async Task<IActionResult> CreatePalletForReceipt(CreateReceiptPlanCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Aktualizacja przyjęcia, poprawa palet -> Post
		[HttpPost("update")]
		public async Task<IActionResult> Update(UpdateReceiptCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Anulowanie przyjęcia, nie kasacja - zostaje ślad - nie ma wpływu na stan
		[HttpPost("cancel")]
		public async Task<IActionResult> Cancel(DeleteReceiptCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Zatwierdzenie skończenia rozładunku - magazyn
		[HttpPost("confirmEnd")]
		public async Task<IActionResult> ConfirmEndReceipt(CompletePhysicalReceiptCommand command)
			=> (await _mediator.Send(command)).ToActionResult();
		
		//Zatwierdzenie rozładunku biuro - zmiana stanu magazynowego, palety w obiekgu
		[HttpPost("finalize")]
		public async Task<IActionResult> FinalizeReceipt(VerifyAndFinalizeReceiptCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Pobranie przyjęcia np do edycji 
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
			=> (await _mediator.Send(new GetReceiptByIdQuery(id))).ToActionResult();

		//Pobranie przyjęć 
		[HttpGet("ReceiptsByFiltr")]
		public async Task<IActionResult> GetByFiltr(GetReceiptsQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}

}
