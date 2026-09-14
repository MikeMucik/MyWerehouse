using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanHandler(IUnitOfWork unitOfWork,
		IReceiptRepo receiptRepo,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<CreateReceiptPlanCommand, AppResult<Unit>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<Unit>> Handle(CreateReceiptPlanCommand request, CancellationToken ct)
		{
			return await _unitOfWork.ExecuteInTransactionAsync(
				async transactionCt =>
				{
					var now = _dateTimeProvider.UtcNow;
					var receiptNumber = await _receiptRepo.GetNextNumberOfReceipt(transactionCt);
					var receipt = Receipt.Create(receiptNumber, request.DTO.ClientId, request.DTO.PerformedBy, request.DTO.RampNumber, now);
					_receiptRepo.AddReceipt(receipt);
					receipt.Create(request.DTO.PerformedBy);
					await _unitOfWork.SaveChangesAsync(transactionCt);
					return AppResult<Unit>.Success(Unit.Value, "Receipt created.");
				}, IsolationLevel.Serializable, ct);
		}
	}
}
