using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanHandler : IRequestHandler<CreateReceiptPlanCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IMediator _mediator;

		public CreateReceiptPlanHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_mediator = mediator;
		}
		public async Task<ReceiptResult> Handle(CreateReceiptPlanCommand request, CancellationToken cancellationToken)
		{
			try
			{				
				var receipt = new Receipt(request.DTO.ClientId, request.DTO.PerformedBy);
				_receiptRepo.AddReceipt(receipt);
				await _werehouseDbContext.SaveChangesAsync();
				await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, request.DTO.PerformedBy), cancellationToken);
				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				return ReceiptResult.Ok("Utworzono przyjęcie", receipt.Id);
			}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}	
}
