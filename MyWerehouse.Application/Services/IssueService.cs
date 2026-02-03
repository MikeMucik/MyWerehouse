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
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Commands.ChangePalletDuringLoading;
using MyWerehouse.Application.Issues.Commands.CompletedIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.Commands.DeleteIssue;
using MyWerehouse.Application.Issues.Commands.FinishIssueNotCompleted;
using MyWerehouse.Application.Issues.Commands.UpdateIssue;
using MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Application.Issues.Queries.GetIssueById;
using MyWerehouse.Application.Issues.Queries.LoadingIssueList;
using MyWerehouse.Application.Issues.Queries.PalletsToTakeOffList;
using MyWerehouse.Application.Issues.Queries.GetIssuesByFiltr;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Receviving.Filters;

namespace MyWerehouse.Application.Services
{
	public class IssueService : IIssueService
	{
		private readonly IMediator _mediator;
		public IssueService(IMediator mediator)
		{
			_mediator = mediator;		
		}		
		public async Task<List<IssueResult>> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, DateTime dateToSend)
		{
			return await _mediator.Send(new CreateNewIssueCommand(createIssueDTO, dateToSend));			
		}
		//public async Task<IssueResult> AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product)// dla jednego rodzaju produktu
		//{
		//	return await _mediator.Send(new AddPalletsToIssueByProductCommand(issue, product));			
		//}
		//pobranie zamówienia do aktualizacji 

		public async Task<UpdateIssueDTO> GetIssueByIdToUpdateAsync(int numberIssue)
		{
			return await _mediator.Send(new GetIssueProductSummaryByIdQuery(numberIssue));			
		}
		public async Task<IssueDTO> GetIssueByIdAsync(int numberIssue)
		{
			return await _mediator.Send(new GetIssueByIdQuery(numberIssue));
		}
		//aktualizacja/poprawienie zamówienia
		public async Task<List<IssueResult>> UpdateIssueAsync(UpdateIssueDTO issueDTO, DateTime dateToSend)
		{
			return await _mediator.Send(new UpdateIssueNewCommand(issueDTO, dateToSend));
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
			//	var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issueDTO.Id);
			//	foreach (var pickingTask in pickingTasks)
			//	{
			//		var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(pickingTask.VirtualPalletId);
			//		var pallet = await _palletRepo.GetPalletByIdAsync(virtualPallet.PalletId);
			//		_pickingTaskRepo.DeletePickingTask(pickingTask);

			//		if (virtualPallet.PickingTasks.Count == 0)
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
		}
		//TODO
		public async Task<IssueResult> CancelIssueAsync(int issueId, string userId)
		{
			return await _mediator.Send(new CancelIssueCommand(issueId, userId));			
		}

		//zweryfikować czy wszystkie produkty zostały zrobione na palety - nie wiem czy taka ręczna walidacja potrzebna
		public async Task<IssueResult> VerifyIssueToLoadAsync(int issueId, string userId)
		{
			return await _mediator.Send(new VerifyIssueToLoadCommand(issueId, userId));			
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
		}
		//zatwierdzenie zakończenia załadunku przez magazyniera
		public async Task<IssueResult> CompletedIssueAsync(int issueId, string confirmedBy)
		{
			return await _mediator.Send(new CompletedIssueCommand(issueId, confirmedBy));			
		}
		//sprawdzenie załadunku i przerzucenie palet załadowanych do archiwum, zmniejszenie zasobu magazynowego
		public async Task<IssueResult> VerifyIssueAfterLoadingAsync(int issueId, string verifyBy)
		{			
			return await _mediator.Send(new VerifyIssueAfterLoadingCommand(issueId, verifyBy));
			
		}
		//podmiana palety podczas załadunku
		public async Task<IssueResult> ChangePalletInIssueAsync(int issueId, string oldPalletId, string newPalletId, string performedBy)
		{
			return await _mediator.Send(new ChangePalletInIssueCommand(issueId, oldPalletId, newPalletId, performedBy));			
		}
		// To jest lista palet do zdjęcia dla wózkowego
		public async Task<IssuePalletsWithLocationDTO> PalletsToTakeOffListAsync(int issueId, string userId)
		{
			return await _mediator.Send(new PalletsToTakeOffListQuery(issueId, userId));			
		}
		public async Task<List<IssueDTO>> GetIssuesByFiltrAsync(IssueReceiptSearchFilter filter)
		{
			return await _mediator.Send(new GetIssuesByFiltrQuery(filter));			
		}


		//Metody pomocnicze
		//private async Task<List<PickingTask>> AddPickingTaskToIssueAsync(Issue issue, int productId, int quantity, DateOnly bestBefore, string userId)
		//{

		//	if (quantity <= 0) return new List<PickingTask>();
		//	var listOfPickingTask = new List<PickingTask>();
		//	var virtualPallets = await _pickingPalletRepo.GetVirtualPalletsAsync(productId);
		//	foreach (var virtualPallet in virtualPallets)
		//	{
		//		var alreadyAllocated = virtualPallet.PickingTasks.Sum(a => a.Quantity);
		//		var availableOnThisPallet = virtualPallet.InitialPalletQuantity - alreadyAllocated;
		//		if (availableOnThisPallet <= 0) continue;
		//		var quantityToTake = Math.Min(quantity, availableOnThisPallet);
		//		var newPickingTask = PickingTaskUtilis.CreatePickingTask(virtualPallet, issue, quantityToTake);
		//		_pickingTaskRepo.AddPickingTask(newPickingTask);
		//		listOfPickingTask.Add(newPickingTask);
		//		issue.PickingTasks.Add(newPickingTask);
		//		quantity -= quantityToTake;
		//		if (quantity <= 0) break;
		//	}
		//	while (quantity > 0)
		//	{
		//		var newVirtualPallet = await _palletService.AddPalletToPickingAsync(issue, productId, bestBefore, userId);
		//		var quantityToTake = Math.Min(quantity, newVirtualPallet.InitialPalletQuantity);
		//		var newPickingTask = PickingTaskUtilis.CreatePickingTask(newVirtualPallet, issue, quantityToTake);
		//		_pickingTaskRepo.AddPickingTask(newPickingTask);
		//		listOfPickingTask.Add(newPickingTask);
		//		quantity -= quantityToTake;
		//	}
		//	return listOfPickingTask;
		//}
	}
}