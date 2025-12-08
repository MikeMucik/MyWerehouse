using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptHandler : IRequestHandler<AddPalletToReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IPublisher _mediator;
		private readonly IMapper _mapper;
		public AddPalletToReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IPalletRepo palletRepo,
			IPublisher mediator,
			IMapper mapper
			)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_palletRepo = palletRepo;
			_mediator = mediator;
			_mapper = mapper;
		}
		public async Task<ReceiptResult> Handle(AddPalletToReceiptCommand request, CancellationToken cancellationToken)
		{
			var receipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId);
			if (receipt == null || (receipt.ReceiptStatus != ReceiptStatus.Planned && receipt.ReceiptStatus != ReceiptStatus.InProgress))
			{
				return ReceiptResult.Fail("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
			}
			using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(cancellationToken))
			{
				var startReceiving = false;
				try
				{
					if (receipt.ReceiptStatus == ReceiptStatus.Planned)
					{
						receipt.ReceiptStatus = ReceiptStatus.InProgress;
						receipt.ReceiptDateTime = DateTime.UtcNow;
						startReceiving = true;
					}
					var pallet = _mapper.Map<Pallet>(request.DTO);
					pallet.ReceiptId =request.ReceiptId;
					pallet.Id = await _palletRepo.GetNextPalletIdAsync();//kolejny numer palety
					pallet.LocationId = 1;//lokalizacja początkowa
					pallet.DateReceived = DateTime.UtcNow;
					pallet.Status = PalletStatus.Receiving;
					_palletRepo.AddPallet(pallet);
					await _werehouseDbContext.SaveChangesAsync(cancellationToken);
					await transaction.CommitAsync(cancellationToken);

					await _mediator.Publish(new CreatePalletOperationNotification(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.Received,
							request.DTO.UserId,
							PalletStatus.Receiving,
							null
						), cancellationToken);

					if (startReceiving)
					{
						await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, request.DTO.UserId), cancellationToken);
					}
					await _werehouseDbContext.SaveChangesAsync(cancellationToken);
					return ReceiptResult.Ok($"Paleta {pallet.Id} została dodana do przyjęcia {request.ReceiptId}", pallet.Id);
				}
				catch (ReceiptException erp)
				{
					await transaction.RollbackAsync(cancellationToken);
					return ReceiptResult.Fail(erp.Message);
				}				
				catch (Exception ex)
				{
					await transaction.RollbackAsync(cancellationToken);
					//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
					return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
				}
			}
		}
	}
}
