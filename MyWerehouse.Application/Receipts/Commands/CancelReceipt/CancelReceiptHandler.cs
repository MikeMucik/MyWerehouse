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
		WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo) : IRequestHandler<CancelReceiptCommand, AppResult<Unit>>
	{
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;


		public async Task<AppResult<Unit>> Handle(CancelReceiptCommand request, CancellationToken ct)
		{
			var receipt = await _receiptRepo.GetReceipForCanceltByIdAsync(request.ReceiptId);
			if (receipt == null) return AppResult<Unit>.Fail($"Receipt {request.ReceiptId} was not found.");

			var listPalletsOfReceipt = await _palletRepo.GetPalletsByReceiptId(request.ReceiptId);
			//logika do domeny
			foreach (var pallet in listPalletsOfReceipt)
			{			
				if (!pallet.CanBeCancelled())
				{
					return AppResult<Unit>.Fail("The receipt cannot be cancelled because its pallets are already in warehouse circulation.", ErrorType.Conflict);
				}
			}
			foreach (var pallet in listPalletsOfReceipt)
			{
				pallet.DetachFromReceipt(request.UserId, pallet.Location.ToSnapshot());
			}
			receipt.Cancel(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Receipt and its pallets were cancelled.");
		}
	}
}
