using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt
{
	public class CompletePhysicalReceiptHandler :IRequestHandler<CompletePhysicalReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IMediator _mediator;
		public CompletePhysicalReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_mediator = mediator;
		}
		public async Task<ReceiptResult> Handle(CompletePhysicalReceiptCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
				if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.InProgress)
				{
					return ReceiptResult.Fail("Nie można zakończyć przyjęcia - błędny status zlecenia");
				}
				receipt.ReceiptStatus = ReceiptStatus.PhysicallyCompleted;
				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				await _mediator.Publish(new CreateHistoryReceiptNotification(request.ReceiptId, receipt.ReceiptStatus, request.UserId), cancellationToken);				
				return ReceiptResult.Ok("Zakończono fizyczne przyjęcie - gotowe do weryfikacji", request.ReceiptId);
			}
			catch (NotFoundReceiptException erp)
			{
				return ReceiptResult.Fail(erp.Message);
			}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}
}
