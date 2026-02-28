using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.DomainExceptions;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public class VerifyAndFinalizeReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo) : IRequestHandler<VerifyAndFinalizeReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<ReceiptResult> Handle(VerifyAndFinalizeReceiptCommand request, CancellationToken cancellationToken)
		{
			
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(cancellationToken);
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
				?? throw new NotFoundReceiptException(request.ReceiptId);			
			//TODO :Dodać porównanie papierów z tym co rzeczywiście przyjęte, compare amount assignment to real receipt
			try
			{
				receipt.VerifiedReceipt(request.UserId);
				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				return ReceiptResult.Ok("Palety z przyjęcia zweryfikowano, gotowe do działania", receipt.ReceiptNumber);

			}
			catch (DomainException de)
			{
				transaction.Rollback(); 
				return ReceiptResult.Fail(de.Message);				
			}
		}
	}
}