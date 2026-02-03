using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking;
using MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class ReversePickingService : IReversePickingService
	{		
		private readonly IMediator _mediator;		
		public ReversePickingService(IMediator mediator)
		{			
			_mediator = mediator;			
		}

		//public async Task CreateTaskToReversePickingAsync(string palletId, string userId)//paleta kompletacyjna różne asortymenty
		//{
			//<List<ReversePickingResult>>
			//return
			//	await _mediator.Send(new CreateTaskToReversePickingCommand(palletId, userId));
			//if (await _reversePickingRepo.ExistsForPickingPalletAsync(palletId))
			//	throw new InvalidOperationException("Zadania dekompletacji są już utworzone.");

			//await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//var listTasks = new List<ReversePicking>();
			//var listResult = new List<ReversePickingResult>();
			//var pallet = await _palletRepo.GetPalletByIdAsync(palletId)
			//	?? throw new NotFoundPalletException(palletId);
			//var issue = pallet.Issue
			//	?? throw new NotFoundIssueException("Brak zlecenia wydania.");
			//var pickingTasksOfPickingPallet = await _pickingTaskRepo.GetPickingTasksByPickingPalletIdAsync(palletId);
			//if (pickingTasksOfPickingPallet.Count == 0) throw new NotFoundAlloactionException("Brak alokacji dla palety. Paleta nie do dekompletacji.");
			//foreach (var pickingTaskToReverse in pickingTasksOfPickingPallet)
			//{
			//	listTasks.Add(new ReversePicking
			//	{
			//		PickingPalletId = palletId,
			//		Quantity = pickingTaskToReverse.Quantity,
			//		ProductId = pickingTaskToReverse.ProductId,
			//		BestBefore = pickingTaskToReverse.BestBefore,
			//		Status = ReversePickingStatus.Pending,
			//		PickingTaskId = pickingTaskToReverse.Id,
			//		UserId = userId,
			//	});
			//	listResult.Add(ReversePickingResult.Ok("Utworzono zadanie dekompletadcji", pickingTaskToReverse.ProductId, palletId));
			//}
			//foreach (var task in listTasks)
			//{
			//	_reversePickingRepo.AddReversePicking(task);
			//}
			//await _werehouseDbContext.SaveChangesAsync();
			//await transaction.CommitAsync();
			//foreach (var task in listTasks)
			//{
			//	var itemHistory = new HistoryReversePickingItem(
			//		task.Id, task.SourcePalletId, task.DestinationPalletId,	issue.Id, 
			//		task.ProductId, task.Quantity,	null,	task.Status);
			//	await _mediator.Publish(new CreateHistoryReversePickingNotification(itemHistory, userId));
			//}
			//return listResult;
		//}
		public async Task<ReversePickingResult> ExecutiveReversePickingAsync(int taskReverseId, ReversePickingStrategy strategy, string? sourcePalletId, string userId, List<Pallet>? pallets)
		{
			return await _mediator.Send(new ExecutiveReversePickingCommand(taskReverseId, strategy, sourcePalletId, userId, pallets));
			//using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			//try
			//{
			//	var result = new ReversePickingResult();
			//	var reversePicking = await _reversePickingRepo.GetReversePickingAsync(taskReverseId);
			//	if (reversePicking is null)
			//	{
			//		return ReversePickingResult.Fail("Brak zadania do dekompletacji");
			//	}
			//	if (strategy == ReversePickingStrategy.AddToExistingPallet)
			//	{
			//		var filter = new PalletSearchFilter
			//		{
			//			ProductId = reversePicking.ProductId,
			//			BestBefore = reversePicking.BestBefore,
			//		};
			//		var addingPallets = _palletRepo.GetPalletsByFilter(filter);
			//		var palletToAdded = await addingPallets
			//				.Where(p => p.ReceiptId != null)//paleta z przyjęcia ma numer przyjęcia
			//				.OrderBy(q => q.ProductsOnPallet.First().Quantity)//paleta z przyjęcia ma tylko jeden asortyment
			//				.FirstOrDefaultAsync();

			//		if (palletToAdded != null)
			//		{
			//			var product = await _productRepo.GetProductByIdAsync(reversePicking.ProductId)
			//				?? throw new NotFoundProductException(reversePicking.ProductId);

			//			if (product.CartonsPerPallet == 0)
			//			{
			//				throw new NotFoundProductException($"Produkt {reversePicking.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
			//			}						
			//			var numberOfCartoons = product.CartonsPerPallet;
			//			if ((palletToAdded.ProductsOnPallet.First().Quantity + reversePicking.Quantity) > numberOfCartoons)
			//			{
			//				strategy = ReversePickingStrategy.AddToNewPallet;
			//			}
			//			else
			//			{
			//				reversePicking.DestinationPalletId = palletToAdded.Id;
			//			}
			//		}
			//		else throw new NotFoundPalletException(palletToAdded.Id);
			//	}

			//	reversePicking.Status = ReversePickingStatus.InProgress;
			//	string? sourcePalletId = null;
			//	string? destinationPalletId = null;
			//	var issueId = reversePicking.PickingTask.IssueId;
			//	if (issueId == 0)
			//		throw new NotFoundIssueException(reversePicking.PickingTask.IssueId);
			//	switch (strategy)
			//	{
			//		case ReversePickingStrategy.ReturnToSource:
			//			sourcePalletId = reversePicking.SourcePalletId;
			//			if (sourcePalletId == null) throw new NotFoundPalletException(sourcePalletId);//problem bo id string
			//			result = AddProductsToSourcePallet(reversePicking, userId);
			//			break;
			//		case ReversePickingStrategy.AddToExistingPallet:
			//			result = await AddToExistingPallet(reversePicking, userId);
			//			destinationPalletId = reversePicking.DestinationPalletId;
			//			break;
			//		case ReversePickingStrategy.AddToNewPallet:
			//			result = AddToNewPallet(reversePicking, userId);
			//			break;
			//	}
			//	reversePicking.Status = ReversePickingStatus.Completed;
			//	await _werehouseDbContext.SaveChangesAsync();
			//	await transaction.CommitAsync();
			//	var history = new HistoryReversePickingItem(reversePicking.Id,
			//		sourcePalletId,
			//		destinationPalletId,
			//		issueId,
			//		reversePicking.Quantity,
			//		reversePicking.ProductId,
			//		ReversePickingStatus.InProgress,
			//		ReversePickingStatus.Completed);
			//	await _mediator.Publish(new CreateHistoryReversePickingNotification(history, userId));
			//	foreach (var evn in _eventCollector.Events)
			//	{
			//		await _mediator.Publish(evn);
			//	}
			//	foreach (var factory in _eventCollector.DeferredEvents)
			//	{
			//		await _mediator.Publish(factory());
			//	}
			//	return result;
			//}
			//catch (NotFoundIssueException ie)
			//{
			//	await transaction.RollbackAsync();
			//	return ReversePickingResult.Fail(ie.Message);
			//}
			//catch (NotFoundPalletException pe)
			//{
			//	await transaction.RollbackAsync();
			//	return ReversePickingResult.Fail(pe.Message);
			//}
			//catch (NotFoundProductException proe)
			//{
			//	await transaction.RollbackAsync();
			//	return ReversePickingResult.Fail(proe.Message);
			//}
			//catch (Exception ex)
			//{
			//	await transaction.RollbackAsync();
			//	// Loguj ex dla developera!
			//	//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
			//	throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			//}
			//finally
			//{
			//	_eventCollector.Clear();
			//}
		}

		public async Task<ReversePickingDetails> GetReversePickingAsync(int reversePickingId)
		{
			return await _mediator.Send(new GetReversePickingToDoQuery(reversePickingId));
			//var reversePicking = await _reversePickingRepo.GetReversePickingAsync(reversePickingId)
			//?? throw new NotFoundReversePickingException(reversePickingId);

			//var reversePickingDTO = _mapper.Map<ReversePickingDTO>(reversePicking);
			//var sourcePallet = reversePicking?.PickingTask.VirtualPallet.Pallet;
			//var exsitingPickingPallet = false;
			//var existingPalletWithProduct = false;
			//if (sourcePallet != null)
			//{
			//	if (sourcePallet.Status != PalletStatus.Archived)
			//	{
			//		exsitingPickingPallet = true;
			//	}
			//}
			//var filter = new PalletSearchFilter
			//{
			//	ProductId = reversePicking.ProductId,
			//	BestBefore = reversePicking.BestBefore,
			//};
			//var addingPallets = _palletRepo.GetPalletsByFilter(filter);
			//var product = await _productRepo.GetProductByIdAsync(reversePicking.ProductId)
			//	?? throw new NotFoundProductException(reversePicking.ProductId);
			//if (product.CartonsPerPallet == 0)
			//{
			//	throw new NotFoundProductException($"Produkt {reversePicking.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
			//}
			////return ReversePickingResult.Fail($"Produkt {reversePicking.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
			//var numberOfCartoons = product.CartonsPerPallet;
			//var palletToAdded = await addingPallets
			//	.Where(p => p.ReceiptId != null && p.Status == PalletStatus.Available
			//	&& p.ProductsOnPallet.First().Quantity < numberOfCartoons)//paleta z przyjęcia ma numer przyjęcia				
			//	.OrderBy(q => q.ProductsOnPallet.First().Quantity)//paleta z przyjęcia ma tylko jeden asortyment
			//	.FirstOrDefaultAsync();
			//if (palletToAdded != null)
			//{
			//	existingPalletWithProduct = true;
			//}
			//var result = new ReversePickingDetails
			//{
			//	ReversePickingDTO = reversePickingDTO,
			//	CanReturnToSource = exsitingPickingPallet,
			//	CanAddToExistingPallet = existingPalletWithProduct,
			//};
			//return result;
		}
		
		//Czy to potrzebne czy może dodać parametr palety?? albo dodać jeszcze jedną szczegółową?
		public async Task<ListReversePickingDTO> GetListReversePickingToDo(int pageSize, int pageNumber, DateOnly start, DateOnly end)
		{
			//var listReverse = await _reversePickingRepo.GetReversePickings()
			return await _mediator.Send(new GetListReversePickingToDoQuery(pageSize, pageNumber, start, end));
			//var listReversePicking = _reversePickingRepo.GetReversePickings()
			//	.Where(r => r.Status == ReversePickingStatus.Pending)
			//	.ProjectTo<ReversePickingDTO>(_mapper.ConfigurationProvider);
			//var listToShow = await listReversePicking
			//	.Skip(pageSize * (pageNumber - 1))
			//	.Take(pageSize)
			//	.ToListAsync();
			//var listReversePickingDTO = new ListReversePickingDTO()
			//{
			//	DTOs = listToShow,
			//	PageSize = pageSize,
			//	CurrentPage = pageNumber,
			//	Count = await listReversePicking.CountAsync()
			//};
			//return listReversePickingDTO;
		}
		//Metody pomocnicze
		//private ReversePickingResult AddProductsToSourcePallet(ReversePicking task, string userId)
		//{
		//	var sourcePallet = task.PickingTask.VirtualPallet.Pallet;
		//	if (sourcePallet.Status != PalletStatus.Archived)
		//	{
		//		sourcePallet.ProductsOnPallet.First().Quantity += task.Quantity;
		//	}
		//	_eventCollector.Add(new CreatePalletOperationNotification(
		//		sourcePallet.Id,
		//		sourcePallet.LocationId,
		//		ReasonMovement.Correction,
		//		userId,
		//		PalletStatus.Available,
		//		null));
		//	return ReversePickingResult.Ok("Dodano towar do palety źródłowej", task.ProductId, task.SourcePalletId);
		//}
		//private async Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, string userId)
		//{
		//	var palletToAdd = await _palletRepo.GetPalletByIdAsync(task.DestinationPalletId) ??
		//	throw new NotFoundPalletException("Brak palety do dodania");
		//	//if (palletToAdd != null)
		//	//{
		//		palletToAdd.ProductsOnPallet.First().Quantity += task.Quantity;
		//	//}
		//	//else throw new NotFoundPalletException("Brak palety do dodania");
		//	_eventCollector.Add(new CreatePalletOperationNotification(
		//		palletToAdd.Id,
		//		palletToAdd.LocationId,
		//		ReasonMovement.Correction,
		//		userId,
		//		PalletStatus.Available,
		//		null));
		//	return ReversePickingResult.Ok("Dodano towar do palety niepełnej", task.ProductId, task.DestinationPalletId);
		//}
		//private ReversePickingResult AddToNewPallet(ReversePicking task, string userId)
		//{
		//	var newPallet = new Pallet
		//	{
		//		DateReceived = DateTime.UtcNow,
		//		LocationId = 1,
		//		Status = PalletStatus.InStock,
		//		ReceiptId = 1000,//to trzeba poprawić żeby taka nowa paleta miała jakieś przyjęcie tylko palety kompletacyjne nie mają ReceiptId
		//		ProductsOnPallet = new List<ProductOnPallet>
		//		{new ProductOnPallet
		//			{
		//				ProductId = task.ProductId,
		//				DateAdded = DateTime.UtcNow,
		//				Quantity = task.Quantity,
		//			 },
		//		},
		//	};
		//	_palletRepo.AddPallet(newPallet);
		//	_eventCollector.AddDeferred(() =>
		//	new CreatePalletOperationNotification(
		//		newPallet.Id, 1, ReasonMovement.New,
		//		userId, PalletStatus.InStock, null));
		//	return ReversePickingResult.Ok("Dodano towar do nowej palety.", task.ProductId, newPallet.Id);
		//}

	}
}