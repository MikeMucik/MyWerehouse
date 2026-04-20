using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class AddPickingTaskToIssueService : IAddPickingTaskToIssueService
	{
		private readonly IProductRepo _productRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		public AddPickingTaskToIssueService(
			IProductRepo productRepo,
			IPickingPalletRepo pickingPalletRepo,
			IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo)
		{
			_productRepo = productRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
		}

		public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet vp, Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId)
		{
			var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated,
				productId, bestBefore, null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
			_pickingTaskRepo.AddPickingTask(pickingTask);
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(vp.PalletId);
			pickingTask.AddHistory(userId, sourcePallet.Id, sourcePallet.PalletNumber, issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
			return AddPickingTaskToIssueResult.Ok(pickingTask);
		}
		public async Task<AddPickingTaskToIssueResult> AddPickingTaskToIssue(List<Pallet> pallets, List<VirtualPallet> virtualPallets, Issue issue, Guid productId, int rest, DateOnly? bestBefore, string userId)
		{
			// pallets - palety używane w danym issue - potrzebne bo jeszcze nie zapisane w bazie - jeden handler - brak saveChanges
			var quantity = rest;
			var pickingTasks = new List<PickingTask>(); //dla result 
														//z dostępnych palet do pickingu
			foreach (var vp in virtualPallets)
			{
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				var pickingTask = PickingTask.Create(vp.Id, issue.Id, taken, PickingStatus.Allocated, productId, bestBefore, null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
				_pickingTaskRepo.AddPickingTask(pickingTask);
				pickingTasks.Add(pickingTask);

				pickingTask.AddHistory(userId, vp.Pallet.Id, vp.Pallet.PalletNumber, issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0); quantity -= taken; if (quantity <= 0) break;
			}
			//nowe palety do pickingu
			var availablePallets = await _palletRepo.GetAvailablePallets(productId, bestBefore).ToListAsync();
			var usedPalletsId = pallets?
				.Select(p => p.Id)
				.ToHashSet() ?? new HashSet<Guid>();
			var availablePalletsReduced = availablePallets
				.Where(p => !usedPalletsId.Contains(p.Id))
				.ToList();
			foreach (var palletToPicking in availablePalletsReduced)
			{
				if (quantity <= 0) break;
				var virtualPallet = VirtualPallet.Create(palletToPicking.Id, palletToPicking.ProductsOnPallet.First().Quantity, palletToPicking.LocationId);
				palletToPicking.AssignToPicking(userId, palletToPicking.Location.ToSnopShot()); //z nowych palet do pickingu
				var vp = _pickingPalletRepo.AddPalletToPicking(virtualPallet);

				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				var pickingTask = PickingTask.Create(vp.Id, issue.Id, taken, PickingStatus.Allocated, productId, bestBefore, null,
					DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);  //na razie ustalone na sztywno 
				_pickingTaskRepo.AddPickingTask(pickingTask);
				pickingTasks.Add(pickingTask);

				pickingTask.AddHistory(userId, palletToPicking.Id, palletToPicking.PalletNumber, issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
				quantity -= taken;
				if (quantity <= 0) break;
			}
			//jeśli za mało towaru to komunikat dla użytkownika i rollback w handlerze
			if (quantity > 0)
			{
				var productFull = await _productRepo.GetProductByIdAsync(productId);
				return AddPickingTaskToIssueResult.Fail($"Nie ma więcej asortymentu {productId}, {productFull.SKU} - nie można utworzyć zadania pickingu.");
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}

	}
}