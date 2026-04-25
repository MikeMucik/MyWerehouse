using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt
{
	public class CompletePhysicalReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo) : IRequestHandler<CompletePhysicalReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(CompletePhysicalReceiptCommand request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null)
				return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);

			receipt.CompletePhysicalReceipt(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
			return AppResult<Unit>.Success(Unit.Value, "Zakończono fizyczne przyjęcie - gotowe do weryfikacji");
		}
	}
}
