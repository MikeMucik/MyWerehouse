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
	public class AddNewPickingTaskToIssueService
	{
		private readonly IProductRepo _productRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPalletRepo _palletRepo;
		public AddNewPickingTaskToIssueService(IProductRepo productRepo, IPickingPalletRepo pickingPalletRepo, IPalletRepo palletRepo)
		{
			_productRepo = productRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_palletRepo = palletRepo;
		}
		public async Task<AddPickingTaskToIssueResult> AddNewPickingTaskToIssue(List<Pallet> pallets, List<VirtualPallet> virtualPallets, Issue issue, Guid productId, int rest, DateOnly? bestBefore, string userId)
		{
			// pallets - palety używane w danym issue - potrzebne bo jeszcze nie zapisane w bazie - jeden handler - brak saveChanges
			var quantity = rest;
			var pickingTasks = new List<PickingTask>(); //dla result
			//z dostępnych palet do pickingu
			foreach (var vp in virtualPallets)
			{
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (vp.RemainingQuantity <= 0) continue;
				var pickingTask = PickingTask.Create(vp.Id, issue.Id, taken, PickingStatus.Allocated, productId, bestBefore,
					null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
				pickingTasks.Add(pickingTask);
				pickingTask.AddHistory(userId, vp.Pallet.Id, vp.Pallet.PalletNumber, issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
				quantity -= taken;
				if (quantity == 0) break;
			}
			//nowe palety do pickingu
			var availablePallets = await _palletRepo.GetAvailablePallets(productId, bestBefore).ToListAsync();
			var usedPalletsId = pallets?
				.Select(p => p.Id)
				.ToHashSet()
				?? new HashSet<Guid>();
			var availablePalletsReduced = availablePallets
				.Where(p => !usedPalletsId.Contains(p.Id))
				.ToList();
			foreach (var palletToPicking in availablePalletsReduced)
			{
				if (quantity == 0) break;
				var virtualPallet = new VirtualPallet
				{
					Pallet = palletToPicking,
					PickingTasks = [],
					DateMoved = DateTime.UtcNow,
					Location = palletToPicking.Location,
					InitialPalletQuantity = palletToPicking.ProductsOnPallet.First().Quantity,
				};
				palletToPicking.AssignToPicking(userId);
				//z nowych palet do pickingu
				var vp = _pickingPalletRepo.AddPalletToPicking(virtualPallet);
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				var pickingTask = PickingTask.Create(vp.Id, issue.Id, taken, PickingStatus.Allocated, productId, bestBefore,
					null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
				pickingTasks.Add(pickingTask);
				pickingTask.AddHistory(userId, palletToPicking.Id, palletToPicking.PalletNumber, issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
				quantity -= taken;
				if (quantity == 0) break;
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
