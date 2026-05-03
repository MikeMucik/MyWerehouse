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
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class AddPickingTaskToIssueService : IAddPickingTaskToIssueService
	{
		private readonly IProductRepo _productRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		public AddPickingTaskToIssueService(
			IProductRepo productRepo,
			IVirtualPalletRepo virtualPalletRepo,
			IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo)
		{
			_productRepo = productRepo;
			_virtualPalletRepo = virtualPalletRepo;
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
		}

		public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet vp, Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId)
		{
			var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated,
				productId, bestBefore, null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
			_pickingTaskRepo.AddPickingTask(pickingTask);
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(vp.PalletId);
			if (sourcePallet == null)
				return AddPickingTaskToIssueResult.Fail("Brak palety źródłowej.");
			pickingTask.AddHistoryPicking(userId, null, null, PickingStatus.Available, 0);// PickingStatus.Allocated, sourcePallet.Id, sourcePallet.PalletNumber
			return AddPickingTaskToIssueResult.Ok(pickingTask);
		}
		public async Task<AddPickingTaskToIssueResult> AddPickingTaskToIssue(List<Pallet> pallets, List<VirtualPallet> virtualPallets,
			Issue issue, Guid productId, int rest, DateOnly? bestBefore, string userId)
		{
			// pallets - palety używane w danym issue - potrzebne bo jeszcze nie zapisane w bazie - jeden handler - brak saveChanges
			var quantity = rest;
			var pickingTasks = new List<PickingTask>(); //dla result 																											
			void CreatePickingTask(VirtualPallet vp, Issue issue, int quantity, Guid productId, DateOnly? bestBefore, string userId)
			{
				var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated, productId,
						bestBefore, null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);  //na razie ustalone na sztywno 
				_pickingTaskRepo.AddPickingTask(pickingTask);
				pickingTasks.Add(pickingTask);

				pickingTask.AddHistoryPicking(userId, null, null, PickingStatus.Available, 0);

			}
			//z dostępnych palet do pickingu	
			foreach (var vp in virtualPallets)
			{
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				CreatePickingTask(vp, issue, taken, productId, bestBefore, userId);
				quantity -= taken;
				if (quantity <= 0)
					break;
			}
			//nowe palety do pickingu
			//TODO weź tylko tyle palet ile potrzebujesz zwiększysz performance na razie Take 10			
			var usedPalletsId = pallets?
				.Select(p => p.Id)
				.ToHashSet() ?? new HashSet<Guid>();
			var availablePallets = await _palletRepo.GetAvailablePalletsExcluding(productId, bestBefore, usedPalletsId);//.ToListAsync();
			
			foreach (var palletToPicking in availablePallets)
			{
				if (quantity <= 0) break;
				var virtualPallet = VirtualPallet.Create(palletToPicking.Id, palletToPicking.ProductsOnPallet.First().Quantity, palletToPicking.LocationId);
				palletToPicking.AssignToPicking(userId, palletToPicking.Location.ToSnapshot()); //z nowych palet do pickingu
				var vp = _virtualPalletRepo.AddPalletToPicking(virtualPallet);

				var taken = Math.Min(quantity, vp.RemainingQuantity);
				if (taken <= 0) continue;
				CreatePickingTask(vp, issue, taken, productId, bestBefore, userId);
				quantity -= taken;
				if (quantity <= 0) break;
			}
			//jeśli za mało towaru to komunikat dla użytkownika 
			if (quantity > 0)
			{
				var productFull = await _productRepo.GetProductByIdAsync(productId);
				return AddPickingTaskToIssueResult.Fail($"Nie ma więcej asortymentu {productId}, {productFull.SKU} - nie można utworzyć zadania pickingu.");
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}

	}
}