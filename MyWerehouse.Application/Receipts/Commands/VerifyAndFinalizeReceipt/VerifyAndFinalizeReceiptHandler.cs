using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt
{
	public class VerifyAndFinalizeReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo) : IRequestHandler<VerifyAndFinalizeReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(VerifyAndFinalizeReceiptCommand request, CancellationToken cancellationToken)
		{

			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(cancellationToken);
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);

			//TODO :Dodać porównanie papierów z tym co rzeczywiście przyjęte, compare amount assignment to real receipt
			
			receipt.VerifiedReceipt(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			return AppResult<Unit>.Success(Unit.Value, "Palety z przyjęcia zweryfikowano, gotowe do działania");
		}
	}
}