using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptHandler : IRequestHandler<UpdateReceiptCommand, ReceiptResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IPublisher _mediator;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IClientRepo _clientRepo;
		private readonly ILocationRepo _locationRepo;
		private readonly ISynchronizerProductsConfig _synchronizerProductsConfig;
		public UpdateReceiptHandler(WerehouseDbContext werehouseDbContext,
			IReceiptRepo receiptRepo,			
			IPublisher mediator,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IClientRepo clientRepo,
			ILocationRepo locationRepo,
			ISynchronizerProductsConfig synchronizerProductsConfig)
		{
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;			
			_mediator = mediator;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_clientRepo = clientRepo;
			_locationRepo = locationRepo;
			_synchronizerProductsConfig = synchronizerProductsConfig;
		}
		public async Task<ReceiptResult> Handle(UpdateReceiptCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				// Palety nie wpływają na stan magazynu do momentu zatwierdzenia przyjęcia
				var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(request.ReceiptId)
					?? throw new NotFoundReceiptException(request.ReceiptId);
				if (existingReceipt.ReceiptStatus == ReceiptStatus.Verified)
				{
					return ReceiptResult.Fail("Zatwierdzone przyjęcie nie może być modyfikowane.");
				}
				if (!await _clientRepo.IsClientExistAsync(request.DTO.ClientId))
				{
					return ReceiptResult.Fail("Wybrany klient nie istnieje.");
				}
				if (!await _locationRepo.ReceivingRampExistsAsync(request.DTO.RampNumber))
					return ReceiptResult.Fail("Wybrana rampa nie istnieje.");
				existingReceipt.ClientId = request.DTO.ClientId;
				existingReceipt.ReceiptStatus = request.DTO.ReceiptStatus;
				existingReceipt.PerformedBy = request.UserId;
				foreach (var item in request.DTO.Pallets)
				{
					if (item.ReceiptId != null && item.ReceiptId != existingReceipt.Id)
					{
						return ReceiptResult.Fail($"Paleta o numerze {item.Id} należy do innego przyjęcia.");
						//throw new PalletException($"Paleta o numerze {item.Id} należy do innego przyjęcia o numerze {item.ReceiptId}");
					}
				}
				//Usuwanie z bazy danych niepotrzebnych pallet
				var incomingPalletsIds = request.DTO.Pallets
					.Select(p => p.Id)
					.Where(id => !string.IsNullOrEmpty(id))
					.ToHashSet();
				var palletToDelete = existingReceipt.Pallets
					.Where(p => !incomingPalletsIds.Contains(p.Id))
					.ToList();
				foreach (var pallet in palletToDelete)//
				{
					_palletRepo.DeletePallet(pallet);
					existingReceipt.Pallets.Remove(pallet);
				}											
				//Aktualizacja produktów na palecie
				var existingPallets = existingReceipt.Pallets.ToDictionary(p => p.Id);
				foreach (var dto in request.DTO.Pallets.Where(p => p.Id != null))
				{
					if (!existingPallets.TryGetValue(dto.Id!, out var pallet))
						continue;
					pallet.Status = PalletStatus.Receiving;
					pallet.DateReceived = DateTime.UtcNow;
					_synchronizerProductsConfig.SynchronizeProducts(pallet, dto.ProductsOnPallet);
				}
				//Dodanie nowych palet
				var palletsAdded = request.DTO.Pallets
					.Where(p=>p.Id == null)
					.ToList();
				foreach (var palletToAdd in palletsAdded)
				{					
						var palletToDo = new Pallet();						
							palletToDo.Id = await _palletRepo.GetNextPalletIdAsync();						
						palletToDo.ReceiptId = request.ReceiptId;
						palletToDo.LocationId = request.DTO.RampNumber;//lokalizacja początkowa rampy
						palletToDo.DateReceived = DateTime.UtcNow;
						palletToDo.Status = PalletStatus.Receiving;
						//Sprawdź czy item.ProductsOnPallet ma elementy
						if (palletToAdd.ProductsOnPallet == null || palletToAdd.ProductsOnPallet.Count == 0)
						{
							//throw new PalletException("Nowa paleta musi mieć co najmniej jeden produkt");							
							throw new DomainPalletException(palletToDo.Id);
						}
						var product = palletToAdd.ProductsOnPallet.Single();//założenie systemowe - paleta przyjmowana jeden produkt - poniżej jakby było inne

						if (!await _productRepo.IsExistProduct(product.ProductId))
							throw new NotFoundProductException(product.ProductId);

						palletToDo.ProductsOnPallet.Add(new ProductOnPallet
						{
							Pallet = palletToDo,
							ProductId = product.ProductId,
							Quantity = product.Quantity,
							BestBefore = product.BestBefore,
							DateAdded = DateTime.UtcNow,
						});
						//For Pallet with many products
						//foreach (var product in item.ProductsOnPallet)
						//{
						//	if (!await _productRepo.IsExistProduct(product.ProductId))
						//		throw new InvalidProductException(product.ProductId);
						//	var productToPallet = new ProductOnPallet
						//	{
						//		Pallet = palletToDo,
						//		ProductId = product.ProductId,
						//		Quantity = product.Quantity,
						//		BestBefore = product.BestBefore,
						//		DateAdded = DateTime.UtcNow,
						//	};
						//	palletToDo.ProductsOnPallet.Add(productToPallet);
						//}
						existingReceipt.Pallets.Add(palletToDo);					
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				foreach (var pallet in existingReceipt.Pallets)
				{
					await _mediator.Publish(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
							ReasonMovement.Correction, request.UserId, PalletStatus.Receiving,null), ct);
				}
				await _mediator.Publish(new CreateHistoryReceiptNotification(existingReceipt.Id, existingReceipt.ReceiptStatus, request.UserId), ct);				
				return ReceiptResult.Ok($"Przyjęcie o numerze {request.ReceiptId} zostało zaktualizowane", request.ReceiptId);
			}
			catch (NotFoundReceiptException exr)
			{
				await transaction.RollbackAsync(ct);
				return ReceiptResult.Fail(exr.Message);
			}
			catch (NotFoundPalletException expal)
			{
				await transaction.RollbackAsync(ct);
				return ReceiptResult.Fail(expal.Message);
			}
			catch(NotFoundProductException epr)
			{
				await transaction.RollbackAsync(ct);
				return ReceiptResult.Fail(epr.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}
}
