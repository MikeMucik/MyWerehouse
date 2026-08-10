using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.CancelReceipt
{
	public class CancelReceiptHandler(IReceiptRepo receiptRepo,
		WerehouseDbContext werehouseDbContext) : IRequestHandler<CancelReceiptCommand, AppResult<Unit>>
	{
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;

		public async Task<AppResult<Unit>> Handle(CancelReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceipForCancelByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");
			receipt.Cancel(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Receipt and its pallets were cancelled.");
		}
	}
}
