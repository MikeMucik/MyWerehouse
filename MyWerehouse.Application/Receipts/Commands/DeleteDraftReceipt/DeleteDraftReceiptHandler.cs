using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.DeleteDraftReceipt
{
	public class DeleteDraftReceiptHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo
		) : IRequestHandler<DeleteDraftReceiptCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(DeleteDraftReceiptCommand request, CancellationToken ct)
		{
				var receipt = await _receiptRepo.GetReceiptOnlyByIdAsync(request.ReceiptId);
				if (receipt == null) return AppResult<Unit>.Fail($"Przyjęcie o numerze {request.ReceiptId} nie zostało znalezione.", ErrorType.NotFound);
				receipt.Delete(request.UserId);
				_receiptRepo.DeleteReceipt(receipt);
				return AppResult<Unit>.Success(Unit.Value, "Usunięto przyjęcie z bazy");		
		}
	}
}
