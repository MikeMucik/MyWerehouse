using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt
{
	public class CompletePhysicalReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo) : IRequestHandler<CompletePhysicalReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<ReceiptResult> Handle(CompletePhysicalReceiptCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)??
					throw new NotFoundReceiptException(request.ReceiptId);				
				receipt.CompletePhysicalReceipt(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				return ReceiptResult.Ok("Zakończono fizyczne przyjęcie - gotowe do weryfikacji", receipt.ReceiptNumber);
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
