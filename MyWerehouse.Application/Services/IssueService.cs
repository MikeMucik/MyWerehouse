using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.VerifyIssueAfterLoading;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Commands.AddPalletsToIssueByProduct;
using MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.Commands.DeleteIssue;
using MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted;
using MyWerehouse.Application.Issues.Commands.UpdateIssue;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Application.Results;
using MyWerehouse.Application.Utils;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.LoadingIssueList;
using MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr;
using MyWerehouse.Application.Inventories.Commands.ChangeQuantity;
using MyWerehouse.Application.Issues.Commands.CancelIssue;

namespace MyWerehouse.Application.Services
{
	public class IssueService : IIssueService
	{
		private readonly IMediator _mediator;

		//private readonly IIssueRepo _issueRepo;
		//private readonly IMapper _mapper;//
		//private readonly WerehouseDbContext _werehouseDbContext;
		//private readonly IPalletRepo _palletRepo;

		//private readonly IAllocationRepo _allocationRepo;
		//private readonly IPickingPalletRepo _pickingPalletRepo;
		public IssueService(
			IMediator mediator
			//,			IIssueRepo issueRepo,
		//	IMapper mapper,
			//WerehouseDbContext werehouseDbContext,
		//	IPalletRepo palletRepo,
		//	IAllocationRepo allocationRepo
		//	, IPickingPalletRepo pickingPalletRepo
			)
		{
			_mediator = mediator;
		//	_issueRepo = issueRepo;
		//	_mapper = mapper;
		//	_werehouseDbContext = werehouseDbContext;
		//	_palletRepo = palletRepo;
		//	_allocationRepo = allocationRepo;
		//	_pickingPalletRepo = pickingPalletRepo;
		}
		//public IssueService(
		//	IIssueRepo issueRepo)
		//{
		//	_issueRepo = issueRepo;
		//}
		public async Task<List<IssueResult>> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, DateTime dateToSend)
		{
			return await _mediator.Send(new CreateNewIssueCommand(createIssueDTO, dateToSend));
			//var validationResult = _createIssueValidator.Validate(createIssueDTO);
			//if (!validationResult.IsValid)
			//{
			//	throw new ValidationException(validationResult.Errors);
			//}
			//var issue = new Issue(createIssueDTO.ClientId, createIssueDTO.PerformedBy, dateToSend);
			//_issueRepo.AddIssue(issue);
			//issue.IssueItems = new List<IssueItem>();
			//var addedProducts = new List<IssueResult>();
			//foreach (var item in createIssueDTO.Items)
			//{
			//	var notAddedProducts = await AddPalletsToIssueByProductAsync(issue, item);
			//	addedProducts.Add(notAddedProducts);
			//	var newItem = new IssueItem
			//	{
			//		ProductId = item.ProductId,
			//		Quantity = item.Quantity,
			//		BestBefore = item.BestBefore,
			//	};
			//	issue.IssueItems.Add(newItem);
			//}
			//if (addedProducts.Any(r => r.Success == false))
			//{
			//	issue.IssueStatus = IssueStatus.NotComplete;
			//}
			//await _werehouseDbContext.SaveChangesAsync();
			//await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, createIssueDTO.PerformedBy));//
			//return addedProducts;
		}
		public async Task<IssueResult> AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product)// dla jednego rodzaju produktu
		{
			return await _mediator.Send(new AddPalletsToIssueByProductCommand(issue, product));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//var totalAvailable = 0;
			//try
			//{
			//	var availablePalletsQuery = await _palletService.GetAllAvailablePalletsAsync(product.ProductId, product.BestBefore);
			//	//totalAvailable = await _inventoryService.GetProductCountAsync(product.ProductId, product.BestBefore);
			//	totalAvailable = await _mediator.Send(new GetProductCountQuery(product.ProductId, product.BestBefore));

			//	if (product.Quantity > totalAvailable)
			//	{
			//		throw new ProductException($"Nie wystarczająca ilości produktu o numerze {product.ProductId}. Asortyment nie został dodany do zlecenia.");
			//	}
			//	var numberUnitOnPallet = await _productRepo.GetProductByIdAsync(product.ProductId) 
			//	?? throw new ProductException($"Produkt {product.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
			//	var number = numberUnitOnPallet.CartonsPerPallet;
			//	var amountPallets = product.Quantity / number; 
			//	var rest = product.Quantity % number;           
			//	issue.IssueStatus = IssueStatus.Pending;
			//	// jeszcze warunek by wybierał najpierw pełne palety - first full pallets
			//	var palletsToAsign = availablePalletsQuery
			//		.OrderByDescending(p => p.ProductsOnPallet.First(po => po.Quantity > 0).Quantity)
			//		.Take(amountPallets)
			//		.ToList();
			//	foreach (var pallet in palletsToAsign)// dodanie do zlecenia pełnych palet - adding full pallets
			//	{
			//		pallet.IssueId = issue.Id;
			//		pallet.Status = PalletStatus.InTransit;
			//		_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id,
			//		pallet.LocationId,
			//		ReasonMovement.ToLoad,
			//		issue.PerformedBy,
			//		PalletStatus.InTransit,
			//		null));
			//		issue.Pallets.Add(pallet);
			//	}
			//	List<Allocation> newAllocationFromRest = new List<Allocation>();
			//	//stworzenie zadania picking dla resztówki jeśli rest < 0 -  making picking for rest
			//	if (rest > 0)
			//	{
			//		newAllocationFromRest = await AddAllocationToIssueAsync(issue, product.ProductId, rest, product.BestBefore, issue.PerformedBy);// palety do pickingu
			//		await _werehouseDbContext.SaveChangesAsync();
			//		foreach (var allocation in newAllocationFromRest)
			//		{
			//			_eventCollector.Add(new CreateHistoryPickingNotification(
			//				allocation.VirtualPalletId,
			//		allocation.Id,
			//		issue.PerformedBy,
			//		PickingStatus.Allocated,
			//		0));
			//		}
			//	}
			//	//await _werehouseDbContext.SaveChangesAsync();
			//	foreach (var evn in _eventCollector.Events)
			//	{
			//		await _mediator.Publish(evn, CancellationToken.None);
			//	}
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();

			//	_eventCollector.Clear();

			//	return IssueResult.Ok("Towar dołączono do wydania", product.ProductId);
			//}
			//catch (ProductException expr)
			//{
			//	await transaction.RollbackAsync();
			//	return IssueResult.Fail(
			//		expr.Message,
			//		product.ProductId,
			//		product.Quantity,
			//		totalAvailable);
			//}
			//catch (PalletNotFoundException expal)
			//{
			//	await transaction.RollbackAsync();
			//	return IssueResult.Fail(
			//		expal.Message,
			//		product.ProductId);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	throw new InvalidOperationException("Wystąpił błąd podczas przypisywania palet do zlecenia.", ex.InnerException);
			//}
		}
		//pobranie zamówienia do aktualizacji 

		public async Task<UpdateIssueDTO> GetIssueByIdToUpdateAsync(int numberIssue)
		{
			return await _mediator.Send(new GetIssueProductSummaryByIdQuery(numberIssue));
			//var issue = await _issueRepo.GetIssueByIdAsync(numberIssue) ?? throw new IssueException(numberIssue);
			//return new UpdateIssueDTO
			//{
			//	Id = issue.Id,
			//	ClientId = issue.ClientId,
			//	PerformedBy = issue.PerformedBy,
			//	Items = issue.Pallets
			//	 .SelectMany(p => p.ProductsOnPallet)
			//	 .Select(prod => new IssueItemDTO { ProductId = prod.ProductId, Quantity = prod.Quantity })
			//	 .ToList(),
			//	DateToSend = issue.IssueDateTimeSend
			//};
		}
		public async Task<IssueDTO> GetIssueByIdAsync(int numberIssue)
		{
			return await _mediator.Send(new GetIssueByIdQuery(numberIssue));
		}
		//aktualizacja/poprawienie zamówienia
		public async Task<List<IssueResult>> UpdateIssueAsync(UpdateIssueDTO issueDTO, DateTime dateToSend)
		{
			return await _mediator.Send(new UpdateIssueCommand(issueDTO, dateToSend));
			//var resultList = new List<IssueResult>();
			//var issue = await _issueRepo.GetIssueByIdAsync(issueDTO.Id) ?? throw new IssueNotFoundException(issueDTO.Id);
			//issue.PerformedBy = issueDTO.PerformedBy;             //1.2 Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone lub nie zaczęty picking
			//if (issue.IssueStatus == IssueStatus.New ||
			//	issue.IssueStatus == IssueStatus.Pending ||
			//	issue.IssueStatus == IssueStatus.NotComplete)
			//{
			//	//usuwanie palet z issue
			//	foreach (var pallet in issue.Pallets.ToList())
			//	{
			//		_palletRepo.ClearPalletFromListIssue(pallet);
			//	}
			//	issue.Pallets.Clear();
			//	//Usuwanie alokacji
			//	var allocations = await _allocationRepo.GetAllocationsByIssueIdAsync(issueDTO.Id);
			//	foreach (var allocation in allocations)
			//	{
			//		var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(allocation.VirtualPalletId);
			//		var pallet = await _palletRepo.GetPalletByIdAsync(virtualPallet.PalletId);
			//		_allocationRepo.DeleteAllocation(allocation);

			//		if (virtualPallet.Allocations.Count == 0)
			//		{
			//			_pickingPalletRepo.DeleteVirtualPalletPicking(virtualPallet);
			//			pallet.Status = PalletStatus.Available;
			//		};
			//	}
			//	foreach (var item in issueDTO.Items)
			//	{
			//		var result = await AddPalletsToIssueByProductAsync(issue, item);
			//		resultList.Add(result);
			//	}
			//	issue.IssueStatus = IssueStatus.ChangingPallet;
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, issue.PerformedBy));//
			//	return resultList;
			//}
			//else if (issue.IssueStatus == IssueStatus.ConfirmedToLoad)
			//{				
			//	var newQuantities = new List<IssueItemDTO>();
			//	var hasNegativeDiff = false;
			//	var errorMessage = new List<string>();
			//	foreach (var product in issueDTO.Items)
			//	{
			//		var productId = product.ProductId;
			//		var oldQuantity = await _issueItemRepo.GetQuantityByIssueAndProduct(issue, productId);
			//		var newQuantity = product.Quantity - oldQuantity;
			//		if (newQuantity < 0)
			//		{
			//			hasNegativeDiff = true;
			//			errorMessage.Add($"Produkt {productId}: Nie można zmniejszyć z {oldQuantity} do {product.Quantity} (różnica : {newQuantity}). Zlecenie jest już zatwierdzone do załadunku");
			//			continue;
			//		}
			//		if (newQuantity > 0)
			//		{
			//			var newItem = new IssueItemDTO
			//			{
			//				ProductId = productId,
			//				Quantity = newQuantity,
			//				BestBefore = product.BestBefore
			//			};
			//			newQuantities.Add(newItem);
			//		}
			//	}
			//	if (hasNegativeDiff)
			//	{
			//		return new List<IssueResult>
			//		{
			//			IssueResult.Fail(string.Join(";", errorMessage),0)
			//		};
			//	}
			//	if (newQuantities.Count == 0)
			//	{
			//		return new List<IssueResult> { IssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.", 0) };
			//	}
			//	var dataForNewIssue = new CreateIssueDTO
			//	{
			//		ClientId = issue.ClientId,
			//		Items = newQuantities,
			//		PerformedBy = issueDTO.PerformedBy,
			//	};
			//	resultList = await CreateNewIssueAsync(dataForNewIssue, issueDTO.DateToSend);
			//	foreach (var result in resultList)
			//	{
			//		if (result.Success)
			//		{
			//			result.Message += " (Dodatkowe zlecenie na ostatnią chwilę - stare jest w realizacji).";
			//		}
			//	}
			//	return resultList;
			//}
			//else
			//{
			//	resultList.Add(IssueResult.Fail(
			//		$"Nie można zaktualizować zlecenia {issue.Id}, status: {issue.IssueStatus}"));
			//	return resultList;
			//}
		}
		//dopracować
		public async Task<IssueResult> DeleteIssueAsync(int issueId, string userId)
		{
			return await _mediator.Send(new DeleteIssueCommand(issueId, userId));
			//await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var issueToDelete = await _issueRepo.GetIssueByIdAsync(issueId)
			//		?? throw new IssueNotFoundException(issueId);

			//	List<INotification> palletList = [];
			//	List<INotification> allocationList = [];
			//	switch (issueToDelete.IssueStatus)
			//	{
			//		case IssueStatus.New:
			//			_issueRepo.DeleteIssue(issueToDelete);
			//			break;

			//		case IssueStatus.Pending:
			//		case IssueStatus.NotComplete:
			//			issueToDelete.IssueStatus = IssueStatus.Cancelled;
			//			foreach (var pallet in issueToDelete.Pallets)
			//			{
			//				pallet.IssueId = null;
			//				pallet.Status = PalletStatus.Available;
			//				palletList.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId, ReasonMovement.Correction, userId, PalletStatus.Available, null));
			//			}
			//			foreach (var allocation in issueToDelete.Allocations)
			//			{
			//				allocation.PickingStatus = PickingStatus.Cancelled;
			//				allocation.Quantity = 0;
			//				allocationList.Add(new CreateHistoryPickingNotification(allocation.VirtualPalletId, allocation.Id, userId, PickingStatus.Allocated, 0));
			//			}
			//			break;
			//			default:
			//			throw new IssueNotFoundException($"Zlecenia {issueToDelete.Id} nie można anulować.");
			//	}				
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	if(!(issueToDelete.IssueStatus == IssueStatus.New)) { 
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issueId, userId));}
			//	foreach (var p in palletList)
			//	{
			//		await _mediator.Publish(p);
			//	}
			//	foreach (var a in allocationList)
			//	{
			//		await _mediator.Publish(a);
			//	}
			//	return IssueResult.Ok($"Usunięto zamówienie o numerze {issueToDelete.Id}.");
			//}
			//catch (IssueNotFoundException ei)
			//{
			//	await transaction.RollbackAsync();
			//	return IssueResult.Fail(ei.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			//}
		}
		//TODO
		public async Task<IssueResult> CancelIssueAsync(int issueId, string userId)
		{
			return await _mediator.Send(new CancelIssueCommand(issueId, userId));
			//await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId)
			//			?? throw new IssueException(issueId);
			//	var listPallet = new List<Pallet>();
			//	//anulowanie zlecenia dla pełnych palet
			//	foreach (var pallet in issue.Pallets)
			//	{
			//		if (pallet.ReceiptId != null)//paleta kompletacyjna nie ma ReceiptId tylko pełne palety z przyjęcia
			//		{
			//			pallet.Status = PalletStatus.Available;
			//			pallet.IssueId = null;
			//			pallet.LocationId = 1;//lokalizacja rampy
			//			listPallet.Add(pallet);
			//		}
			//	}
			//	//palety kompletacyjne i zadania pickingu
			//	var restPallets = issue.Pallets.Except(listPallet).ToList();

			//	foreach (var p in restPallets)
			//	{
			//		if (p.Status == PalletStatus.Picking)
			//		{
			//			//zadanie do reversePicking
			//		}
			//	}
			//	//usuń alokacje jeśli nie zrobione
			//	var allocations = await _allocationRepo.GetAllocationsByIssueIdAsync(issueId);
			//	foreach (var allocation in allocations)
			//	{
			//		_allocationRepo.DeleteAllocation(allocation);
			//	}
			//	//usuń virtualPallet jeśli należy tylko do tego zlecenia
			//	var virtualPallets = await _allocationRepo.GetVirtualPalletsByIssue(issueId);
			//	foreach (var vp in virtualPallets)
			//	{
			//		if (vp.Allocations.Count == 0)
			//		{
			//			_pickingPalletRepo.DeleteVirtualPalletPicking(vp);
			//		}
			//	}
			//	issue.IssueStatus = IssueStatus.Cancelled;
			//	issue.PerformedBy = userId;
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issueId, userId));
			//	foreach (var p in listPallet)
			//	{
			//		await _mediator.Publish(new CreatePalletOperationNotification(p.Id, 1, ReasonMovement.CancelIssue, userId, PalletStatus.Available, null));
			//	}
			//	return IssueResult.Ok($"Anulowano zlecenie {issueId}.");
			//}
			//catch (IssueException ie)
			//{
			//	await transaction.RollbackAsync();
			//	return IssueResult.Fail(ie.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			//}
		}

		//zweryfikować czy wszystkie produkty zostały zrobione na palety - nie wiem czy taka ręczna walidacja potrzebna
		public async Task<IssueResult> VerifyIssueToLoadAsync(int issueId, string userId)
		{
			return await _mediator.Send(new VerifyIssueToLoadCommand(issueId, userId));
			//await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId)
			//			?? throw new IssueNotFoundException(issueId);
			//	issue.IssueStatus = IssueStatus.ConfirmedToLoad;
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issueId, userId));//
			//	return IssueResult.Ok("Wydanie zatwierdzono.", issueId);
			//}
			//catch (IssueNotFoundException ei)
			//{
			//	await transaction.RollbackAsync();
			//	return IssueResult.Fail(ei.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			//}
		}
		//To jest lista do załadunku dla magazyniera lub dla biura do podmian palet
		public async Task<ListPalletsToLoadDTO> LoadingIssueListAsync(int issueId, string userId)
		{
			return await _mediator.Send(new LoadingIssueListQuery(issueId, userId));
			//var issue = await _issueRepo.GetIssueByIdAsync(issueId)
			//	?? throw new IssueException(issueId);
			////zebrać palety po wysyłki 		trzeba się zastanowić czy status tylko ToIssue					 
			//return new ListPalletsToLoadDTO
			//{
			//	IssueId = issueId,
			//	ClientId = issue.ClientId,
			//	ClientName = issue.Client.Name,
			//	Pallets = issue.Pallets
			//	.Where(p => p.Status == PalletStatus.InTransit ||
			//	p.Status == PalletStatus.InStock ||
			//	p.Status == PalletStatus.Available ||
			//	p.Status == PalletStatus.ToIssue)
			//	.Select(p => new PalletToLoadDTO
			//	{
			//		PalletId = p.Id,
			//		LocationName = (p.Location.Bay + " " + p.Location.Aisle + " " + p.Location.Position + " " + p.Location.Height).ToString(),
			//		PalletStatus = p.Status,
			//		LocationId = p.LocationId,
			//		ProductOnPalletIssue = p.ProductsOnPallet.Select(pp => new ProductOnPalletIssueDTO
			//		{
			//			ProductId = pp.ProductId,
			//			ProductName = pp.Product.Name,
			//			SKU = pp.Product.SKU,
			//			BestBefore = pp.BestBefore,
			//			Quantity = pp.Quantity,
			//		}).ToList()
			//	}).OrderBy(p => p.LocationId)
			//	.ToList()
			//};
		}
		//zatwierdzenie pojedynczej palety że jest już załadowana - status
		public async Task<IssueResult> MarkAsLoadedAsync(string palletId, string sendedBy)
		{
			return await _mediator.Send(new MarkAsLoadedCommand(palletId, sendedBy));
			//var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			//if (!(pallet.Status == PalletStatus.ToIssue || pallet.Status == PalletStatus.InTransit || pallet.Status == PalletStatus.Available ||
			//	pallet.Status == PalletStatus.InStock))
			//{ throw new PalletException("Paleta nie ma statusu do załadowania"); }
			//pallet.Status = PalletStatus.Loaded;
			//await _werehouseDbContext.SaveChangesAsync();
			//await _mediator.Publish(new CreatePalletOperationNotification(
			//		pallet.Id,
			//		pallet.LocationId,
			//		ReasonMovement.Loaded,
			//		sendedBy,
			//		PalletStatus.Loaded,
			//		null));
			////await _werehouseDbContext.SaveChangesAsync();
		}
		// zamyka biuro a nie magazyn w przypadku gdy np. załadunek sie nie mieści
		public async Task<IssueResult> FinishIssueNotCompleted(int issueId, string performedBy)
		{
			return await _mediator.Send(new FinishIssueNotCompletedCommand(issueId, performedBy));
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueException(issueId);
			//	var palletsReturn = new List<Pallet>();
			//	foreach (var pallet in issue.Pallets.ToList())
			//	{
			//		if (pallet.Status != PalletStatus.Loaded)
			//		{
			//			pallet.Status = PalletStatus.Available;
			//			pallet.IssueId = null;
			//			issue.Pallets.Remove(pallet);
			//			palletsReturn.Add(pallet);
			//		}
			//	}
			//	issue.IssueStatus = IssueStatus.IsShipped;
			//	issue.PerformedBy = performedBy;
			//	await _werehouseDbContext.SaveChangesAsync();
			//	foreach (var pallet in issue.Pallets)
			//	{
			//		await _mediator.Publish(new CreatePalletOperationNotification(
			//				pallet.Id,
			//				pallet.LocationId,
			//				ReasonMovement.ToLoad,
			//				performedBy,
			//				PalletStatus.Loaded,
			//				null
			//			));
			//		foreach (var product in pallet.ProductsOnPallet)
			//		{
			//			await _mediator.Send(new ChangeQuantityCommand(product.ProductId, -product.Quantity));
			//		}
			//	}
			//	foreach (var pallet in palletsReturn)
			//	{
			//		await _mediator.Publish(new CreatePalletOperationNotification(
			//				pallet.Id,
			//				pallet.LocationId,
			//				ReasonMovement.Correction,
			//				performedBy,
			//				PalletStatus.Available,
			//				null
			//			));
			//	}
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issueId, performedBy));
			//	await _werehouseDbContext.SaveChangesAsync();
			//	return IssueResult.Ok($"Zamknięto wydanie {issueId}.");
		}
		//zatwierdzenie zakończenia załadunku przez magazyniera
		public async Task<IssueResult> CompletedIssueAsync(int issueId, string confirmedBy)
		{
			return await _mediator.Send(new CompletedIssueCommand(issueId, confirmedBy));
			//var issue = await _issueRepo.GetIssueByIdAsync(issueId) ?? throw new IssueException(issueId);
			//foreach (var pallet in issue.Pallets)
			//{
			//	if (pallet.Status != PalletStatus.Loaded)
			//	{
			//		throw new IssueException("Nie załadowano wszystkich palet.");
			//	}
			//}
			//issue.IssueStatus = IssueStatus.IsShipped;
			//await _werehouseDbContext.SaveChangesAsync();
			//await _mediator.Publish(new CreateHistoryIssueNotification(issueId, confirmedBy));
			//return IssueResult.Ok($"Zakończono załadunek {issueId}.");
		}
		//sprawdzenie załadunku i przerzucenie palet załadowanych do archiwum, zmniejszenie zasobu magazynowego
		public async Task<IssueResult> VerifyIssueAfterLoadingAsync(int issueId, string verifyBy)
		{
			//if (_mediator != null)
			return await _mediator.Send(new VerifyIssueAfterLoadingCommand(issueId, verifyBy));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId)
			//			?? throw new IssueException(issueId);
			//	issue.PerformedBy = verifyBy;
			//	if (issue.IssueStatus != IssueStatus.IsShipped) throw new IssueException("Nie zakończono załadunku.");
			//	issue.IssueStatus = IssueStatus.Archived;

			//	foreach (var pallet in issue.Pallets)
			//	{
			//		pallet.Status = PalletStatus.Archived;
			//		await _mediator.Publish(new CreatePalletOperationNotification(
			//				pallet.Id,
			//				pallet.LocationId,
			//				ReasonMovement.ToLoad,
			//				issue.PerformedBy,
			//				PalletStatus.Archived,
			//				null
			//			));

			//		foreach (var product in pallet.ProductsOnPallet)
			//		{
			//			await _mediator.Send(new ChangeQuantityCommand(product.ProductId, -product.Quantity));
			//		}
			//	}
			//	await _mediator.Publish(new CreateHistoryIssueNotification(issueId, verifyBy));//
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	return IssueResult.Ok("Załadunek zatwierdzony, zasoby uaktulanione.");
			//}
			//catch (IssueException ei)
			//{
			//	await transaction.RollbackAsync();
			//	return IssueResult.Fail(ei.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	return IssueResult.Fail("Wystąpił nieoczenikawy błąd przy weryfikacji");
			//}
		}
		//podmiana palety podczas załadunku
		public async Task<IssueResult> ChangePalletInIssueAsync(int issueId, string oldPalletId, string newPalletId, string performedBy)
		{
			return await _mediator.Send(new ChangePalletInIssueCommand(issueId, oldPalletId, newPalletId, performedBy));
			//try
			//{
			//	if (oldPalletId == newPalletId)
			//	{
			//		throw new PalletException("Nie można podmienić paletę na tą samą");
			//	}
			//	var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			//	if (issue == null) throw new IssueException(issueId);
			//	var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(oldPalletId);
			//	var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(newPalletId);
			//	if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
			//	{
			//		throw new PalletException("Jedna z podanych palet nie istnieje.");
			//	}
			//	if (palletToRemoveFromIssue.IssueId != issueId)
			//	{
			//		throw new PalletException("Paleta do usunięcia nie należy do zlecenia.");
			//	}
			//	if (palletToAddingIssue.IssueId != null ||
			//		(palletToAddingIssue.Status != PalletStatus.Available &&
			//		palletToAddingIssue.Status != PalletStatus.InStock))
			//	{
			//		throw new PalletException("Nowej palety nie można przypisać do zlecenia, błędny status.");
			//	}
			//	var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			//	var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			//	if (productOnOldPallet != productOnNewPallet)
			//	{
			//		throw new PalletException("Nie można podmienić palet z różnymi produktami.");
			//	}
			//	palletToAddingIssue.IssueId = issue.Id;
			//	palletToAddingIssue.Status = PalletStatus.InTransit;
			//	issue.Pallets.Add(palletToAddingIssue);

			//	palletToRemoveFromIssue.IssueId = null;
			//	palletToRemoveFromIssue.Status = PalletStatus.Available;
			//	issue.Pallets.Remove(palletToRemoveFromIssue);
			//	issue.IssueStatus = IssueStatus.ChangingPallet;
			//	issue.PerformedBy = performedBy;
			//	await _mediator.Publish(new CreatePalletOperationNotification(
			//			palletToRemoveFromIssue.Id,
			//			palletToRemoveFromIssue.LocationId,
			//			ReasonMovement.Correction,
			//			issue.PerformedBy,
			//			PalletStatus.Available,
			//			null
			//		));
			//	await _mediator.Publish(new CreatePalletOperationNotification(
			//				palletToAddingIssue.Id,
			//				palletToAddingIssue.LocationId,
			//				ReasonMovement.ToLoad,
			//				issue.PerformedBy,
			//				PalletStatus.ToIssue,
			//				null
			//			));

			//	await _mediator.Publish(new CreateHistoryIssueNotification(issueId, performedBy));
			//	await _werehouseDbContext.SaveChangesAsync();
			//	return IssueResult.Ok("Podmieniono palety.", productOnOldPallet.Value);
			//}
			//catch (PalletException ep)
			//{
			//	return IssueResult.Fail(ep.Message);
			//}
			//catch (IssueException ei)
			//{
			//	return IssueResult.Fail(ei.Message);

			//}
			//catch (Exception ex)
			//{
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	return IssueResult.Fail("Operacaja się nie powiodła.");
			//}
		}
		// To jest lista palet do zdjęcia dla wózkowego
		public async Task<IssuePalletsWithLocationDTO> PalletsToTakeOffListAsync(int issueId, string userId)
		{
			return await _mediator.Send(new PalletsToTakeOffListQuery(issueId, userId));
			//var list = await _issueRepo.GetPalletByIssueIdAsync(issueId);
			////może tu jakieś potwierdzenie że zweryfikowano do załadunku
			//var listToShow = new IssuePalletsWithLocationDTO
			//{
			//	IssueId = issueId,
			//	PalletList = list
			//};
			//return listToShow;
		}
		public async Task<List<IssueDTO>> GetIssuesByFiltrAsync(IssueReceiptSearchFilter filter)
		{
			return await _mediator.Send(new GetIssuesByFiltrQuery(filter));
			//try
			//{
			//	var issues = await _issueRepo.GetIssuesByFilter(filter).ToListAsync();
			//	return _mapper.Map<List<IssueDTO>>(issues);
			//}
			//catch (Exception ex)
			//{
			//	//_logger.LogError(ex, "Error fetching issues.");
			//	return new List<IssueDTO>();
			//}
		}


		//Metody pomocnicze
		//private async Task<List<Allocation>> AddAllocationToIssueAsync(Issue issue, int productId, int quantity, DateOnly bestBefore, string userId)
		//{

		//	if (quantity <= 0) return new List<Allocation>();
		//	var listOfAllocation = new List<Allocation>();
		//	var virtualPallets = await _pickingPalletRepo.GetVirtualPalletsAsync(productId);
		//	foreach (var virtualPallet in virtualPallets)
		//	{
		//		var alreadyAllocated = virtualPallet.Allocations.Sum(a => a.Quantity);
		//		var availableOnThisPallet = virtualPallet.IssueInitialQuantity - alreadyAllocated;
		//		if (availableOnThisPallet <= 0) continue;
		//		var quantityToTake = Math.Min(quantity, availableOnThisPallet);
		//		var newAllocation = AllocationUtilis.CreateAllocation(virtualPallet, issue, quantityToTake);
		//		_allocationRepo.AddAllocation(newAllocation);
		//		listOfAllocation.Add(newAllocation);
		//		issue.Allocations.Add(newAllocation);
		//		quantity -= quantityToTake;
		//		if (quantity <= 0) break;
		//	}
		//	while (quantity > 0)
		//	{
		//		var newVirtualPallet = await _palletService.AddPalletToPickingAsync(issue, productId, bestBefore, userId);
		//		var quantityToTake = Math.Min(quantity, newVirtualPallet.IssueInitialQuantity);
		//		var newAllocation = AllocationUtilis.CreateAllocation(newVirtualPallet, issue, quantityToTake);
		//		_allocationRepo.AddAllocation(newAllocation);
		//		listOfAllocation.Add(newAllocation);
		//		quantity -= quantityToTake;
		//	}
		//	return listOfAllocation;
		//}
	}
}