using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.DomainExceptions;
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
		private readonly IClientRepo _clientRepo = clientRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<AppResult<Unit>> Handle(CreateReceiptPlanCommand request, CancellationToken ct)
		{
			try
			{
				if (!await _clientRepo.IsClientExistAsync(request.DTO.ClientId))
					return AppResult<Unit>.Fail($"Klient o numerze {request.DTO.ClientId} nie istnieje.", ErrorType.NotFound);
				if (!await _locationRepo.ReceivingRampExistsAsync(request.DTO.RampNumber))
					return AppResult<Unit>.Fail("Wybrana rampa nie istnieje.", ErrorType.NotFound);
				var receiptNumber = await _receiptRepo.GetNextNumberOfReceipt();//
				var receipt = Receipt.Create(receiptNumber, request.DTO.ClientId, request.DTO.PerformedBy, request.DTO.RampNumber);
				_receiptRepo.AddReceipt(receipt);
				receipt.Create(request.DTO.PerformedBy);
				await _werehouseDbContext.SaveChangesAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Utworzono przyjęcie");
			}
			//to nie tu to chyba domena
			catch (InvalidUserIdException eu)
			{
				return AppResult<Unit>.Fail(eu.Message);
			}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				//return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				throw;
			}
		}
	}
}
