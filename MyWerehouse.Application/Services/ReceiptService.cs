using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.Inventories.Commands.ChangeQuantity;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Application.Receipts.Commands.CompletePhysicalReceipt;
using MyWerehouse.Application.Receipts.Commands.CreateReceipt;
using MyWerehouse.Application.Receipts.Commands.DeleteReceipt;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using MyWerehouse.Application.Receipts.Commands.VerifyAndFinalizeReceipt;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.Receipts.Events.CreateHistoryReceipt;
using MyWerehouse.Application.Receipts.Queries.GetReceipt;
using MyWerehouse.Application.Receipts.Queries.GetReceipts;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.HistoryDTO;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class ReceiptService : IReceiptService
	{
		private readonly IMediator _mediator;		
		
		public ReceiptService(IMediator mediator)			
		{
			_mediator = mediator;					
		}
		
		public async Task<ReceiptResult> CreateReceiptPlanAsync(CreateReceiptPlanDTO createReceipt)
		{
			return await _mediator.Send(new CreateReceiptPlanCommand(createReceipt));
			//try
			//{
			//	var validationResult = _receiptNewValidator.Validate(createReceipt);
			//	if (!validationResult.IsValid)
			//	{
			//		throw new ValidationException(validationResult.Errors);
			//	}
			//	var receipt = new Receipt(createReceipt.ClientId, createReceipt.PerformedBy);
			//	_receiptRepo.AddReceipt(receipt);
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await _mediator.Publish(new CreateHistoryReceiptCommand(receipt.Id, receipt.ReceiptStatus, createReceipt.PerformedBy));
			//	await _werehouseDbContext.SaveChangesAsync();
			//	return ReceiptResult.Ok("Utworzono przyjęcie", receipt.Id);
			//}
			//catch (Exception ex)
			//{
			//	//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
			//	return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			//}
		}
		public async Task<ReceiptResult> AddPalletToReceiptAsync(int receiptId, CreatePalletReceiptDTO newPalletDto)
		{			
			return await _mediator.Send(new AddPalletToReceiptCommand(receiptId, newPalletDto));
			//var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			//if (receipt == null || (receipt.ReceiptStatus != ReceiptStatus.Planned && receipt.ReceiptStatus != ReceiptStatus.InProgress))
			//{
			//	throw new ReceiptNotFoundException("Nie można dodać palety zły status przyjęcia lub brak utworzenia przyjęcia");
			//}
			//var validationResult = _palletValidator.Validate(newPalletDto);
			//if (!validationResult.IsValid)
			//{
			//	throw new ValidationException(validationResult.Errors);
			//}
			//using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())
			//{
			//	var startReceiving = false;
			//	try
			//	{
			//		if (receipt.ReceiptStatus == ReceiptStatus.Planned)
			//		{
			//			receipt.ReceiptStatus = ReceiptStatus.InProgress;
			//			receipt.ReceiptDateTime = DateTime.UtcNow;
			//			startReceiving = true;
			//		}
			//		var pallet = _mapper.Map<Pallet>(newPalletDto);
			//		pallet.ReceiptId = receiptId;
			//		pallet.Id = await _palletRepo.GetNextPalletIdAsync();//kolejny numer palety
			//		pallet.LocationId = 1;//lokalizacja początkowa
			//		pallet.DateReceived = DateTime.UtcNow;
			//		pallet.Status = PalletStatus.Receiving;
			//		_palletRepo.AddPallet(pallet);
			//		await _werehouseDbContext.SaveChangesAsync();
			//		await transaction.CommitAsync();

			//		await _mediator.Publish(new CreatePalletOperationNotification(
			//				pallet.Id,
			//				pallet.LocationId,
			//				ReasonMovement.Received,
			//				newPalletDto.UserId,
			//				PalletStatus.Receiving,
			//				null
			//			));

			//		if (startReceiving)
			//		{
			//			await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, newPalletDto.UserId));
			//		}
			//		await _werehouseDbContext.SaveChangesAsync();
			//		return ReceiptResult.Ok($"Paleta {pallet.Id} została dodana do przyjęcia {receiptId}", pallet.Id);
			//	}
			//	catch (ReceiptNotFoundException erp)
			//	{
			//		await transaction.RollbackAsync();
			//		return ReceiptResult.Fail(erp.Message);
			//	}
			//	catch (ValidationException ev)
			//	{
			//		await transaction.RollbackAsync();
			//		return ReceiptResult.Fail(ev.Message);
			//	}
			//	catch (Exception ex)
			//	{
			//		await transaction.RollbackAsync();
			//		//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
			//		return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			//	}
			//}
		}
		public async Task<ReceiptResult> CompletePhysicalReceiptAsync(int receiptId, string userId)
		{
			return await _mediator.Send(new CompletePhysicalReceiptCommand(receiptId, userId));
			//try
			//{
			//	var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			//	if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.InProgress)
			//	{
			//		throw new ReceiptNotFoundException("Nie można zakończyć przyjęcia");
			//	}
			//	receipt.ReceiptStatus = ReceiptStatus.PhysicallyCompleted;				
			//	await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, userId));
			//	await _werehouseDbContext.SaveChangesAsync();
			//	return ReceiptResult.Ok("Zakończono fizyczne przyjęcie - gotowe do weryfikacji", receiptId);
			//}
			//catch (ReceiptNotFoundException erp)
			//{
			//	return ReceiptResult.Fail(erp.Message);
			//}
			//catch (Exception ex)
			//{
			//	//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
			//	return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			//}
		}
		public async Task<ReceiptResult> VerifyAndFinalizeReceiptAsync(int receiptId, string userId)
		{
			return	await  _mediator.Send(new VerifyAndFinalizeReceiptCommand(receiptId, userId));
			//using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())
			//{
			//	try
			//	{
			//		var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			//		if (receipt == null || receipt.ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			//		{
			//			throw new ReceiptNotFoundException("Nie można zweryfikować przyjęcia");
			//		}
			//		//logika zatwierdzenia dokumentów
			//		receipt.ReceiptStatus = ReceiptStatus.Verified;
			//		receipt.ReceiptDateTime = DateTime.UtcNow;
			//		foreach (var pallet in receipt.Pallets)
			//		{
			//			pallet.Status = PalletStatus.InStock;
			//			foreach (var product in pallet.ProductsOnPallet)
			//			{							
			//				await _mediator.Send(new ChangeQuantityCommand(product.ProductId, product.Quantity));
			//			}
			//		}
			//		//await _werehouseDbContext.SaveChangesAsync();								
			//		foreach (var pallet in receipt.Pallets)
			//		{
			//			await _mediator.Publish(new CreatePalletOperationNotification(
			//					pallet.Id,
			//					pallet.LocationId,
			//					ReasonMovement.Received,
			//					receipt.PerformedBy,
			//					PalletStatus.InStock,
			//					null
			//				));
			//		}
			//		await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, userId));
			//		await _werehouseDbContext.SaveChangesAsync();

			//		await transaction.CommitAsync();
			//		return ReceiptResult.Ok("Palety z przyjęcia zweryfikowano, gotowe do działania", receiptId);
			//	}
			//	catch (ReceiptNotFoundException erp)
			//	{
			//		await transaction.RollbackAsync();
			//		return ReceiptResult.Fail(erp.Message);
			//	}
			//	catch (Exception ex)
			//	{
			//		await transaction.RollbackAsync();
			//		//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
			//		return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			//	}
			//}
		}
		public async Task<ReceiptDTO> GetReceiptDTOAsync(int receiptId)
		{
			var a = await _mediator.Send(new GetReceiptByIdQuery(receiptId));
			return a;
			//try
			//{
			//var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			//var receiptDTO = _mapper.Map<ReceiptDTO>(receipt);
			//return receiptDTO;
			//}
			//catch (Exception ex)
			//{
			//	//_logger.LogError(ex, "Error fetching issues.");
			//	return null;
			//}
		}
		public async Task<ReceiptResult> UpdateReceiptPalletsAsync(ReceiptDTO updatingReceipt, string userId)
		{
			return await _mediator.Send(new UpdateReceiptCommand(updatingReceipt.Id ,updatingReceipt, userId));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var existingReceipt = await _receiptRepo.GetReceiptByIdAsync(updatingReceipt.Id);
			//	if (existingReceipt == null) throw new ReceiptNotFoundException("Nie znaleziono przyjęcia");

			//	var validationResult = _receiptValidator.Validate(updatingReceipt);
			//	if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

			//	existingReceipt.ClientId = updatingReceipt.ClientId;
			//	existingReceipt.ReceiptStatus = updatingReceipt.ReceiptStatus;
			//	existingReceipt.PerformedBy = userId;
			//	existingReceipt.ReceiptDateTime = DateTime.UtcNow;
			//	foreach (var item in updatingReceipt.Pallets)
			//	{
			//		if (!string.IsNullOrEmpty(item.Id) && item.ReceiptId != updatingReceipt.Id && item.ReceiptId != null)
			//		{
			//			throw new PalletNotFoundException($"Paleta o numerze {item.Id} należy do innego przyjęcia o numerze {item.ReceiptId}");
			//		}
			//	}
			//	var palletsRaw = new List<Pallet>();
			//	foreach (var item in updatingReceipt.Pallets)
			//	{
			//		var pallet = await _palletRepo.GetPalletByIdAsync(item.Id)
			//		?? throw new PalletNotFoundException($"Nie znaleziono palety o Id: {item.Id}");
			//		palletsRaw.Add(pallet);
			//	}
			//	//Usuwanie z bazy danych niepotrzebnych pallet
			//	var incomingPalletsIds = updatingReceipt.Pallets
			//		.Select(p => p.Id)
			//		.Where(id => !string.IsNullOrEmpty(id))
			//		.ToHashSet();
			//	var palletToDelete = existingReceipt.Pallets
			//		.Where(p => !incomingPalletsIds.Contains(p.Id))
			//		.ToList();
			//	foreach (var pallet in palletToDelete)//
			//	{
			//		_palletRepo.DeletePallet(pallet);
			//	}

			//	foreach (var pallet in palletsRaw)
			//	{
			//		var dto = updatingReceipt.Pallets.First(p => p.Id == pallet.Id);
			//		pallet.ReceiptId = existingReceipt.Id;//
			//		pallet.Status = PalletStatus.Receiving;//
			//		pallet.DateReceived = DateTime.UtcNow;					
			//		SynchronizeProducts(pallet, dto.ProductsOnPallet);
			//	}
			//	foreach (var pallet in palletsRaw)
			//	{
			//		if (!existingReceipt.Pallets.Any(x => x.Id == pallet.Id))
			//			existingReceipt.Pallets.Add(pallet);
			//	}				
			//	await _werehouseDbContext.SaveChangesAsync();
			//	foreach (var pallet in palletsRaw)
			//	{
			//		await _mediator.Publish(new CreatePalletOperationNotification(
			//				pallet.Id,
			//				pallet.LocationId,
			//				ReasonMovement.Correction,
			//				userId,
			//				PalletStatus.Receiving,
			//				null
			//			));
			//	}
			//	await _mediator.Publish(new CreateHistoryReceiptNotification(existingReceipt.Id, existingReceipt.ReceiptStatus, userId));

			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	return ReceiptResult.Ok($"Przyjęcie o numerze {updatingReceipt.Id} zostało zaktualizowane", updatingReceipt.Id);

			//}
			//catch (ReceiptNotFoundException exr)
			//{
			//	await transaction.RollbackAsync();
			//	return ReceiptResult.Fail(exr.Message);
			//}
			//catch (PalletNotFoundException expal)
			//{
			//	await transaction.RollbackAsync();
			//	return ReceiptResult.Fail(
			//		expal.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
			//	return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			//}
		}
		public async Task<ReceiptResult> CancelReceiptAsync(int receiptId, string userId)
		{
			return await _mediator.Send(new DeleteReceiptCommand(receiptId, userId));
			//using (var transaction = await _werehouseDbContext.Database.BeginTransactionAsync())
			//{
			//	try
			//	{
			//		var receipt = await _receiptRepo.GetReceiptByIdAsync(receiptId);
			//		if (receipt == null)
			//		{
			//			throw new ReceiptNotFoundException($"Brak przyjęcia o numerze{receiptId}");
			//		}
			//		if (receipt.ReceiptStatus == ReceiptStatus.Verified)
			//		{
			//			throw new ReceiptNotFoundException("Nie można usunąć zweryfikowanego przyjęcia");
			//		}
			//		if (!(receipt.ReceiptStatus == ReceiptStatus.Planned
			//			|| receipt.ReceiptStatus == ReceiptStatus.InProgress
			//			|| receipt.ReceiptStatus == ReceiptStatus.PhysicallyCompleted))
			//		{
			//			throw new ReceiptNotFoundException("Nieprawidłowy status przyjęcia");
			//		}
			//		receipt.ReceiptStatus = ReceiptStatus.Cancelled;
			//		receipt.PerformedBy = userId;
			//		receipt.ReceiptDateTime = DateTime.UtcNow;

			//		//_historyService.CreateHistoryReceipt(receipt, ReceiptStatus.Cancelled, userId);
			//		//usuwanie palet które jeszcze nie weszły w "życie" magazynu
			//		var flag = true;
			//		//if ()
			//		foreach (var pallet in receipt.Pallets.ToList())
			//		{
			//			//dodaj sprawdzenie czy można usunąć paletę??
			//			//var canDelete =  _palletMovementRepo.CanDeleteAsync(pallet.Id);
			//			_palletRepo.DeletePallet(pallet);
			//		}
			//		//usunięcie całkowite przyjęcia- zostawienie tylko snapshotów
			//		//_receiptRepo.DeleteReceipt(receipt);
			//		//await _werehouseDbContext.SaveChangesAsync();

			//		await _mediator.Publish(new CreateHistoryReceiptNotification(receipt.Id, receipt.ReceiptStatus, userId));
			//		//_receiptRepo.DeleteReceipt(receipt); !!!!!!!!!!!!!! problem chyba w DbContext
			//		await _werehouseDbContext.SaveChangesAsync();
			//		await transaction.CommitAsync();
			//		return ReceiptResult.Ok("Usunięto przyjęcie", receiptId);
			//	}
			//	catch (ReceiptNotFoundException erp)
			//	{
			//		await transaction.RollbackAsync();
			//		return ReceiptResult.Fail(erp.Message);
			//	}
			//	catch (Exception ex)
			//	{
			//		await transaction.RollbackAsync();
			//		//_logger.LogError(ex, "Błąd podczas operacji na przyjęciu");
			//		return ReceiptResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			//	}
			//}
		}
		public async Task<List<ReceiptDTO>> GetReceiptDTOsAsync(IssueReceiptSearchFilter filter)
		{
			return await _mediator.Send(new GetReceiptsQuery(filter));
			//try
			//{
			//	var receipts = await _receiptRepo.GetReceiptByFilter(filter).ToListAsync();
			//	return _mapper.Map<List<ReceiptDTO>>(receipts);
			//}
			//catch (Exception ex)
			//{
			//	//_logger.LogError(ex, "Error fetching issues.");
			//	return new List<ReceiptDTO>();
			//}
		}
		//public void SynchronizerProducts(Pallet pallet, IEnumerable<ProductOnPalletDTO> productDto)
		//{
		//	foreach (var dto in productDto)
		//	{
		//		dto.PalletId = pallet.Id;
		//		var existing = pallet.ProductsOnPallet
		//			.FirstOrDefault(p => p.ProductId == dto.ProductId && p.PalletId == dto.PalletId);

		//		if (existing != null)
		//		{
		//			dto.Id = existing.Id; // przypisz faktyczne Id, jeśli istnieje
		//		}
		//	}
		//	CollectionSynchronizer.SynchronizeCollection(
		//		pallet.ProductsOnPallet,
		//		productDto,
		//		product =>  product.Id,				
		//		dto =>  dto.Id,
		//		dto => _mapper.Map<ProductOnPallet>(dto),
		//	(dto, entity) => _mapper.Map(dto, entity)
		//		);
		//}
	}
}
