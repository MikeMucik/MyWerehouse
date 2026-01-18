using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanHandler(WerehouseDbContext werehouseDbContext,
		IReceiptRepo receiptRepo,
		IMediator mediator,
		IClientRepo clientRepo,
		ILocationRepo locationRepo) : IRequestHandler<CreateReceiptPlanCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo = receiptRepo;
		private readonly IMediator _mediator = mediator;
		private readonly IClientRepo _clientRepo = clientRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<ReceiptResult> Handle(CreateReceiptPlanCommand request, CancellationToken ct)
		{
			try
			{
				if (!await _clientRepo.IsClientExistAsync(request.DTO.ClientId))
					return ReceiptResult.Fail($"Klient o numerze {request.DTO.ClientId} nie istnieje.");
				if (!await _locationRepo.ReceivingRampExistsAsync(request.DTO.RampNumber))
					return ReceiptResult.Fail("Wybrana rampa nie istnieje.");
				//var userID = _currentUser.Id; // np. z Claims

				var receipt = new Receipt(request.DTO.ClientId, request.DTO.PerformedBy, request.DTO.RampNumber);
				_receiptRepo.AddReceipt(receipt);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, request.DTO.PerformedBy), ct);
				await _werehouseDbContext.SaveChangesAsync(ct);
				return ReceiptResult.Ok("Utworzono przyjęcie", receipt.Id);
			}
			catch (ClientNotFoundException ei)
			{
				return ReceiptResult.Fail(ei.Message);
			}
			catch (InvalidUserIdException eu)
			{
				return ReceiptResult.Fail(eu.Message);
			}
			//catch (RampNotFoundException er)
			//{
			//	return ReceiptResult.Fail(er.Message);
			//}
			catch (Exception ex)
			{
				//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}	
}
