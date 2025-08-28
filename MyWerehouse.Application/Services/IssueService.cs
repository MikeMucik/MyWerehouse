using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Application.Services
{
	public class IssueService : IIssueService
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletMovementService _palletMovementService;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;


		public IssueService(
			IIssueRepo issueRepo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IPalletMovementService palletMovementService,
			IInventoryRepo inventoryRepo,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IPickingPalletRepo pickingPalletRepo)
		{
			_issueRepo = issueRepo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_palletMovementService = palletMovementService;
			_inventoryRepo = inventoryRepo;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_pickingPalletRepo = pickingPalletRepo;
		}
		public async Task<int> CreateNewIssueAsync(CreateIssueDTO createIssueDTO, string userId, DateTime dateToSend)
		{
			var issue = _mapper.Map<Issue>(createIssueDTO);
			issue.IssueDateTimeCreate = DateTime.UtcNow;
			issue.IssueStatus = IssueStatus.New;
			issue.IssueDateTimeSend = dateToSend;
			await _issueRepo.AddIssueAsync(issue);

			foreach (var item in createIssueDTO.Items)
			{
				await AddPalletsToIssueByProductAsync(issue, item, userId);
			}
			await _palletMovementService.CreateHistoryIssueAsync(issue, IssueStatus.New, userId, null);
			await _werehouseDbContext.SaveChangesAsync();
			return issue.Id;
		}
		public async Task AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product, string userId)// dla jednego produktu
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();

			try
			{
				//var availablePalletsQuery = await _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore)
				//		.ToListAsync(); //dostępne palety					

				var availablePalletsQuery = _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore); //do teestów z Mockami

				var totalAvailable = availablePalletsQuery //całkowita ilość kartonów po parametrach
						.SelectMany(p => p.ProductsOnPallet)
						.Where(p => p.ProductId == product.ProductId)
						.Sum(i => i.Quantity);

				if (product.Quantity > totalAvailable)
				{
					throw new InvalidOperationException($"Brak wystarczającej ilości towaru o id {product.ProductId}.Potrzeba: {product.Quantity}, Dostępne: {totalAvailable}");
				}
				var numberUnitOnPallet = await _productRepo.GetProductByIdAsync(product.ProductId);//ilość kartonów pełnej palety
				if (numberUnitOnPallet == null) { throw new InvalidDataException("Produkt nie ma ustawionej ilosci kartonów na paletę."); }
				var number = numberUnitOnPallet.CartonsPerPallet;
				var amountPallets = product.Quantity / number; //liczba palet
				var rest = product.Quantity % number;           //liczba kartonów

				issue.IssueStatus = IssueStatus.InProgress;
				issue.PerformedBy = userId;
				//var palletsToAsign = availablePalletsQuery.Take(amountPallets);

				// jeszcze warunek by wybierał najpierw pełne palety

				var palletsToAsign = availablePalletsQuery
					.OrderByDescending(p => p.ProductsOnPallet.First(po => po.Quantity > 0).Quantity)
					//.OrderBy(p=>p.ProductsOnPallet.First(p=>p.Quantity == number))
					.Take(amountPallets)
					.ToList();

				foreach (var pallet in palletsToAsign)// dodanie do zlecenia pełnych palet
				{
					pallet.IssueId = issue.Id;
					pallet.Status = PalletStatus.InTransit;
					await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.ToLoad, issue.PerformedBy, null);
					issue.Pallets.Add(pallet);
				}
				await _werehouseDbContext.SaveChangesAsync();
				await AddAllocationToIssueAsync(issue.Id, product.ProductId, rest, product.BestBefore, userId);// pakety do pickingu
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj błąd
				throw new InvalidOperationException("Wystąpił błąd podczas przypisywania palet do zlecenia.", ex);
			}
		}
		
		private async Task AddAllocationToIssueAsync(int issueId, int productId, int quantity, DateOnly bestBefore, string userId)
		{
			if (quantity <= 0) return;

			var pickingPallets = await _pickingPalletRepo.GetPickingPalletsAsync(productId);
			foreach (var pickingPallet in pickingPallets)
			{
				var alreadyAllocated = pickingPallet.Allocation.Sum(a => a.Quantity);
				var availableOnThisPallet = pickingPallet.IssueInitialQuantity - alreadyAllocated;
				if (availableOnThisPallet <= 0) continue;
				var quantityToTake = Math.Min(quantity, availableOnThisPallet);
				await _pickingPalletRepo.AddAllocationAsync(pickingPallet, issueId, quantityToTake);
				quantity -= quantityToTake;
				if (quantity <= 0) break;
			}
			await _werehouseDbContext.SaveChangesAsync();
			if (quantity > 0)
			{
				await AddPalletToPickingAsync(issueId, productId, quantity, bestBefore, userId);
				await AddAllocationToIssueAsync(issueId, productId, quantity, bestBefore, userId);
			}
		}
		
		private async Task AddPalletToPickingAsync(int issueId, int productId, int quantity, DateOnly bestBefore, string userId)
		{
			var newPalletsToPicking = await _palletRepo.GetAvailablePallets(productId, bestBefore).ToListAsync();
			var newPallet = newPalletsToPicking.First();
			if (newPallet == null) throw new InvalidOperationException("Brak palet do pickingu");
			await _pickingPalletRepo.AddPalletToPickingAsync(newPallet.Id);
			await _palletRepo.ChangePalletStatusAsync(newPallet.Id, PalletStatus.ToPicking); //zmiana statusu dla palety
			await _palletMovementService.CreateMovementAsync(newPallet, newPallet.LocationId, ReasonMovement.Picking, userId, null);
			await _werehouseDbContext.SaveChangesAsync();//by wykluczyć wzięcie palety do Issue i dodać paletę do pickingu			
		}
		public async Task<IssueToUpdateDTO> GetIssueByIdAsync(int numberIssue)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(numberIssue);
			if (issue == null) throw new InvalidDataException($"Nie ma zamówienia o numerze {numberIssue}");
			var updatingIssue = _mapper.Map<IssueToUpdateDTO>(issue);
			return updatingIssue;
		}
		public async Task UpdateIssueAsync(int numberIssue, string perfomedBy, ListProductsOfIssue products, DateTime dateToSend)
		{
			var issueToUpdate = await _issueRepo.GetIssueByIdAsync(numberIssue); //pobranie wydania
																				 //1.2 Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone
			if (issueToUpdate.IssueStatus == IssueStatus.New || issueToUpdate.IssueStatus == IssueStatus.InProgress)
			{
				foreach (var pallet in issueToUpdate.Pallets.ToList())
				{
					await _palletRepo.ClearPalletFromListIssueAsync(pallet.Id);
				}
				foreach (var item in products.Values)
				{
					await AddPalletsToIssueByProductAsync(issueToUpdate, item, perfomedBy);
				}
				await _palletMovementService.CreateHistoryIssueAsync(issueToUpdate, issueToUpdate.IssueStatus, perfomedBy, null);
			}
			else if (issueToUpdate.IssueStatus == IssueStatus.ConfirmedToLoad)//3. Dodanie dodatkowego zlecenia do załdaunku - czyli dwa zlecenia na załadunek
			{
				var dataForNewIssue = new CreateIssueDTO
				{
					ClientId = issueToUpdate.ClientId,
					Items = products.Values,
					PerformedBy = perfomedBy,
				};
				await CreateNewIssueAsync(dataForNewIssue, perfomedBy, dateToSend);
			}
			else throw new InvalidOperationException($"Nie można atkualizować zlecenia o numerze {issueToUpdate.Id}");
		}
		public async Task DeleteIssueAsync(int issueId)
		{
			var issueToDelete = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issueToDelete.IssueStatus != IssueStatus.New) throw new InvalidOperationException($"Nie można wykasować zlecenia o numerze {issueId}");
			await _issueRepo.DeleteIssueAsync(issueId);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task VerifyIssueToLoadAsync(int issueId, string userId)
		{
			//zweryfikować czy wszystkie produkty zostały zrobione na palety
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null) throw new InvalidOperationException($"Brak zlecenia o numerze{issueId}");
			issue.IssueStatus = IssueStatus.ConfirmedToLoad;
			await _palletMovementService.CreateHistoryIssueAsync(issue, IssueStatus.ConfirmedToLoad, userId, null);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<ListPalletsToLoadDTO> LoadingIssueAsync(int issueId, string sendedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			//zebrać palety po wysyłki 							 
			return new ListPalletsToLoadDTO
			{
				IssueId = issueId,
				ClientId = issue.ClientId,
				ClientName = issue.Client.Name,
				Pallets = issue.Pallets
				.Where(p => p.Status == PalletStatus.InTransit ||
				p.Status == PalletStatus.InStock ||
				p.Status == PalletStatus.Available)
				.Select(p => new PalletToLoadDTO
				{
					PalletId = p.Id,
					LocationName = (p.Location.Bay + " " + p.Location.Aisle + " " + p.Location.Position + " " + p.Location.Height).ToString(),
					PalletStatus = p.Status,
					ProductOnPalletIssue = p.ProductsOnPallet.Select(pp => new ProductOnPalletIssueDTO
					{
						ProductId = pp.Id,
						ProductName = pp.Product.Name,
						SKU = pp.Product.SKU,
						BestBefore = pp.BestBefore,
						Quantity = pp.Quantity,
					}).ToList()
				}).OrderBy(p => p.LocationId)
				.ToList()
			};
		}
		public async Task MarkAsLoadedAsync(string palletId, string sendedBy)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId);
			if (!(pallet.Status == PalletStatus.InTransit || pallet.Status == PalletStatus.Available ||
				pallet.Status == PalletStatus.InStock))
			{ throw new InvalidOperationException("Paleta nie ma statusu do załadowania"); }
			pallet.Status = PalletStatus.Loaded;
			await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Loaded, sendedBy, null);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task FinishIssueNotCompleted(int issueId, string performedBy)// zamyka biuro a nie magazyn
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null) throw new InvalidOperationException($"Brak zlecenia o numerze{issueId}");
			foreach (var pallet in issue.Pallets.ToList())
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					pallet.Status = PalletStatus.Available;
					pallet.IssueId = null;
					issue.Pallets.Remove(pallet);
					await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Correction, performedBy, null);
				}
				else
				{
					await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Loaded, performedBy, null);
					foreach (var product in pallet.ProductsOnPallet)
					{
						await _inventoryRepo.DecreaseInventoryQuantityAsync(product.ProductId, product.Quantity);
					}
				}
			}
			issue.IssueStatus = IssueStatus.IsShipped;
			await _palletMovementService.CreateHistoryIssueAsync(issue, IssueStatus.IsShipped, performedBy, null);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task CompletedIssueAsync(int issueId, string confirmedBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			foreach (var pallet in issue.Pallets)
			{
				if (pallet.Status != PalletStatus.Loaded)
				{
					throw new InvalidOperationException("Nie załadowano wszystkich palet.");
				}
			}
			issue.IssueStatus = IssueStatus.IsShipped;
			await _palletMovementService.CreateHistoryIssueAsync(issue, IssueStatus.IsShipped, confirmedBy, null);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task VerifyIssueAfterLoadingAsync(int issueId, string verifyBy)
		{
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue.IssueStatus != IssueStatus.IsShipped) throw new InvalidOperationException("Nie zakończono załadunku.");
			issue.IssueStatus = IssueStatus.Archived;
			foreach (var pallet in issue.Pallets)
			{
				pallet.Status = PalletStatus.Archived;
				foreach (var product in pallet.ProductsOnPallet)
				{
					await _inventoryRepo.DecreaseInventoryQuantityAsync(product.ProductId, product.Quantity);
					await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.Archived, verifyBy, null);
				}
			}
			await _palletMovementService.CreateHistoryIssueAsync(issue, IssueStatus.Archived, verifyBy, null);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task ChangePalletDuringLoadingAsync(int issueId, string oldPalletId, string newPalletId, string performedBy)
		{
			if (oldPalletId == newPalletId)
			{
				throw new InvalidOperationException("Nie można podmienić paletę na tą samą");
			}
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null)
			{
				throw new KeyNotFoundException($"Zlecenie o numerze {issueId} nie istnieje");
			}
			var palletToRemoveFromIssue = await _palletRepo.GetPalletByIdAsync(oldPalletId);
			var palletToAddingIssue = await _palletRepo.GetPalletByIdAsync(newPalletId);
			if (palletToAddingIssue == null || palletToRemoveFromIssue == null)
			{
				throw new KeyNotFoundException("Jedna z podanych palet nie iestnieje.");
			}
			if (palletToRemoveFromIssue.IssueId != issueId)
			{
				throw new InvalidOperationException("Paleta do usunięcia nie należy do zlecenia.");
			}
			if (palletToAddingIssue.IssueId != null ||
				(palletToAddingIssue.Status != PalletStatus.Available &&
				palletToAddingIssue.Status != PalletStatus.InStock))
			{
				throw new InvalidOperationException("Nowej palety nie można przypisać do zlecenia.");
			}
			var productOnOldPallet = palletToRemoveFromIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			var productOnNewPallet = palletToAddingIssue.ProductsOnPallet.FirstOrDefault()?.ProductId;
			if (productOnOldPallet != productOnNewPallet)
			{
				throw new InvalidOperationException("Nie można podmienić palet z różnymi produktami.");
			}
			palletToAddingIssue.IssueId = issue.Id;
			palletToAddingIssue.Status = PalletStatus.InTransit;
			issue.Pallets.Add(palletToAddingIssue);

			palletToRemoveFromIssue.IssueId = null;
			palletToRemoveFromIssue.Status = PalletStatus.Available;
			issue.Pallets.Remove(palletToRemoveFromIssue);
			await _palletMovementService.CreateHistoryIssueAsync(issue, IssueStatus.ChangingPallet, performedBy, null);
			await _palletMovementService.CreateMovementAsync(palletToRemoveFromIssue, palletToRemoveFromIssue.LocationId, ReasonMovement.Correction, performedBy, null);
			await _palletMovementService.CreateMovementAsync(palletToAddingIssue, palletToAddingIssue.LocationId, ReasonMovement.ToLoad, performedBy, null);

			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<IssuePalletsWithLocationDTO> PalletsToTakeOffList(int issueId, string userId)
		{
			var list = await _issueRepo.GetPalletByIssueIdAsync(issueId);
			var listToShow = new IssuePalletsWithLocationDTO
			{
				IssueId = issueId,
				PalletList = list
			};
			return listToShow;
		}
	}
}
//private static async Task<List<Pallet>> SelectPalletsForIssueAsync(IQueryable<Pallet> pallets, int quantity)
//{
//	var palletsToIssue = await pallets
//		.Include(p => p.ProductsOnPallet)
//		.ToListAsync();
//	var result = new List<Pallet>();
//	int collected = 0;

//	foreach (var pallet in palletsToIssue)
//	{
//		var productOnPallet = pallet.ProductsOnPallet.FirstOrDefault();
//		if (productOnPallet == null)
//			continue;
//		collected += productOnPallet.Quantity;
//		result.Add(pallet);
//		if (collected >= quantity)
//		{
//			if (collected > quantity)
//			{
//				pallet.Status = PalletStatus.ToPicking;
//			}
//			break;
//		}
//	}
//	return result;


//public async Task AddPalletsToIssueByProductAsync(Issue issue, IssueItemDTO product)// dla jednego produktu
//{
//	var availablePalletsQuery = _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore);
//	var totalAvailable = availablePalletsQuery
//		.SelectMany(p => p.ProductsOnPallet)
//			.Where(p => p.ProductId == product.ProductId)
//			.Sum(i => i.Quantity);
//	if (product.Quantity > totalAvailable)
//	{
//		throw new InvalidOperationException($"Brak wystarczającej ilości towaru id {product.ProductId}");
//	}
//	var numberUnitOnPallet = await _productRepo.GetProductByIdAsync(product.ProductId);
//	if (numberUnitOnPallet == null) { throw new Exception(""); } //do zrobienia
//	var number = numberUnitOnPallet.CartonsPerPallet;
//	var amountPallets = product.Quantity / number;
//	var rest = product.Quantity % number;

//	//var palletsToAsign = await SelectedRequiredPalletsAsync(availablePalletsQuery, amountPallets, product.ProductId);
//	var palletsToAsign = availablePalletsQuery.Take(amountPallets);
//	//trzeba sprawdzić chyba z inventory ile jest dostępnych kartonów
//	//int totalCollected = palletsToAsign.Sum(p => p.ProductsOnPallet.First().Quantity); // lub bardziej dokładnie
//	//if (totalCollected < product.Quantity)
//	//{
//	//	throw new InvalidOperationException($"Brak wystarczającej ilości towaru id {product.ProductId}");
//	//}
//	issue.IssueStatus = IssueStatus.InProgress;
//	foreach (var pallet in palletsToAsign)
//	{
//		pallet.IssueId = issue.Id;
//		pallet.Status = PalletStatus.InTransit;

//		//if (totalCollected > product.Quantity && pallet == palletsToAsign.Last())
//		//{
//		//	pallet.Status = PalletStatus.ToPicking;
//		//}
//		await _palletMovementService.CreateMovementAsync(pallet, pallet.LocationId, ReasonMovement.ToLoad, issue.PerformedBy, null);
//		issue.Pallets.Add(pallet);
//	}
//	await AddAllocationToIssue(issue.Id, product.ProductId, rest, product.BestBefore);
//}