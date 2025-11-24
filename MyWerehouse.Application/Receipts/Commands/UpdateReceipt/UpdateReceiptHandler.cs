using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptHandler : IRequestHandler<UpdateReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IMapper _mapper;
		private readonly IPublisher _mediator;
		private readonly IPalletRepo _palletRepo;
		private readonly ISynchronizerProductsConfig _synchronizerProductsConfig;
		public UpdateReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,
			IMapper mapper,
			IPublisher mediator,
			IPalletRepo palletRepo,
			ISynchronizerProductsConfig synchronizerProductsConfig)			
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_mapper = mapper;
			_mediator = mediator;
			_palletRepo = palletRepo;
			_synchronizerProductsConfig = synchronizerProductsConfig;		
		}
		public async Task<ReceiptResult> Handle(UpdateReceiptCommand request, CancellationToken cancellationToken)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(cancellationToken);
			try
			{
				var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
					?? throw new ReceiptNotFoundException("Nie znaleziono przyjęcia");
				existingReceipt.ClientId = request.Receipt.ClientId;
				existingReceipt.ReceiptStatus = request.Receipt.ReceiptStatus;
				existingReceipt.PerformedBy = request.UserId;
				existingReceipt.ReceiptDateTime = DateTime.UtcNow;
				foreach (var item in request.Receipt.Pallets)
				{
					if (!string.IsNullOrEmpty(item.Id) && item.ReceiptId != request.ReceiptId && item.ReceiptId != null)
					{
						throw new PalletNotFoundException($"Paleta o numerze {item.Id} należy do innego przyjęcia o numerze {item.ReceiptId}");
					}
				}
				var palletsRaw = new List<Pallet>();
				foreach (var item in request.Receipt.Pallets)
				{
					var pallet = await _palletRepo.GetPalletByIdAsync(item.Id)
					?? throw new PalletNotFoundException($"Nie znaleziono palety o Id: {item.Id}");
					palletsRaw.Add(pallet);
				}
				//Usuwanie z bazy danych niepotrzebnych pallet
				var incomingPalletsIds = request.Receipt.Pallets
					.Select(p => p.Id)
					.Where(id => !string.IsNullOrEmpty(id))
					.ToHashSet();
				var palletToDelete = existingReceipt.Pallets
					.Where(p => !incomingPalletsIds.Contains(p.Id))
					.ToList();
				foreach (var pallet in palletToDelete)//
				{
					_palletRepo.DeletePallet(pallet);
				}

				foreach (var pallet in palletsRaw)
				{
					var dto = request.Receipt.Pallets.First(p => p.Id == pallet.Id);
					pallet.ReceiptId = existingReceipt.Id;//
					pallet.Status = PalletStatus.Receiving;//
					pallet.DateReceived = DateTime.UtcNow;
					_synchronizerProductsConfig.SynchronizeProducts(pallet, dto.ProductsOnPallet);
				}
				foreach (var pallet in palletsRaw)
				{
					if (!existingReceipt.Pallets.Any(x => x.Id == pallet.Id))
						existingReceipt.Pallets.Add(pallet);
				}
				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				foreach (var pallet in palletsRaw)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.Correction,
						request.UserId,
							PalletStatus.Receiving,
							null
						), cancellationToken);
				}
				await _mediator.Publish(new CreateHistoryReceiptNotification(existingReceipt.Id, existingReceipt.ReceiptStatus, request.UserId), cancellationToken);

				await _werehouseDbContext.SaveChangesAsync(cancellationToken);
				await transaction.CommitAsync(cancellationToken);
				return ReceiptResult.Ok($"Przyjęcie o numerze {request.ReceiptId} zostało zaktualizowane", request.ReceiptId);
			}
			catch (ReceiptNotFoundException exr)
			{
				await transaction.RollbackAsync(cancellationToken);
				return ReceiptResult.Fail(exr.Message);
			}
			catch (PalletNotFoundException expal)
			{
				await transaction.RollbackAsync(cancellationToken);
				return ReceiptResult.Fail(
					expal.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(cancellationToken);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}
}
