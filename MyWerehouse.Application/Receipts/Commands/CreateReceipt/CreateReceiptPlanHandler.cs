using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IClientRepo clientRepo,
		ILocationRepo locationRepo) : IRequestHandler<CreateReceiptPlanCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;

		public async Task<AppResult<Unit>> Handle(CreateReceiptPlanCommand request, CancellationToken ct)
		{
			var receiptNumber = await _receiptRepo.GetNextNumberOfReceipt();//
				var receipt = Receipt.Create(receiptNumber, request.DTO.ClientId, request.DTO.PerformedBy, request.DTO.RampNumber);
				_receiptRepo.AddReceipt(receipt);
				receipt.Create(request.DTO.PerformedBy);
				await _werehouseDbContext.SaveChangesAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Utworzono przyjęcie.");
		}
	}
}