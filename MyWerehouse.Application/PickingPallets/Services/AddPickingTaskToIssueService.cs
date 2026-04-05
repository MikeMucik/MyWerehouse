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
		public AddPickingTaskToIssueService(
			IProductRepo productRepo,
			IPickingPalletRepo pickingPalletRepo, IPalletRepo palletRepo)
		{			
			_productRepo = productRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_palletRepo = palletRepo;
		}
		public async Task<AddPickingTaskToIssueResult> AddPickingTaskToIssue(List<Pallet> pallets, List<VirtualPallet> virtualPallets, Issue issue, Guid productId, int rest, DateOnly? bestBefore, string userId)
		{
			var pickingTasks = new List<PickingTask>();
			var quantity = rest;
			//async Task AllocateFromVirtualPallet(VirtualPallet vp)
			void AllocateFromVirtualPallet(VirtualPallet vp)
			{
				if (quantity <= 0 || vp.RemainingQuantity <= 0) return;
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated,productId, bestBefore,
					null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
				//var pickingTask = CreatePickingTask(issue,					
				//	taken, vp, productId, bestBefore);
				pickingTasks.Add(pickingTask);
				vp.PickingTasks.Add(pickingTask);
				pickingTask.SetVirtualPalletEntity(vp);// taka zaślepka żeby działało
				//pickingTask.VirtualPallet = vp;
				quantity -= taken;
			}
			foreach (var vp in virtualPallets)
			{
				AllocateFromVirtualPallet(vp);
				if (quantity == 0) break;
			}
			if (quantity > 0)
			{
				//czy potrzeba pobierać drugi raz dostępne palety - tak bo od dostępnych odejmuje już przyłączone do zlecenia
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
					if (quantity <= 0) break;
					var virtualPallet = new VirtualPallet
					{
						Pallet = palletToPicking,
						PickingTasks = [],
						DateMoved = DateTime.UtcNow,
						Location = palletToPicking.Location,
						InitialPalletQuantity = palletToPicking.ProductsOnPallet.First().Quantity,
					};
					palletToPicking.AssignToPicking(userId);
					//await _pickingPalletRepo.AddPalletToPickingAsync(virtualPallet);
					 _pickingPalletRepo.AddPalletToPicking(virtualPallet);
					AllocateFromVirtualPallet(virtualPallet);
					//pickingTask.AddHistory(userId, sourcePallet.Id, sourcePallet.PalletNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
				}
			}
			if (quantity > 0)
			{
				var productFull = await _productRepo.GetProductByIdAsync(productId);
				return AddPickingTaskToIssueResult.Fail($"Nie ma więcej asortymentu {productId}, {productFull.SKU} - nie można utworzyć zadania pickingu.");
			}
			foreach (var pickingTask in pickingTasks)
			{
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(pickingTask.VirtualPallet.PalletId);
				pickingTask.AddHistory(userId, sourcePallet.Id, sourcePallet.PalletNumber,issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);
				//pickingTask.AddHistory(userId, PickingStatus.Available, PickingStatus.Allocated, 0);				
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}

		public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet vp, Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId)
		{
			var pickingTask = PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated,
				productId, bestBefore, null, DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
			//var pickingTask = CreatePickingTask(issue,				
			//	quantity, virtualPallet, productId, bestBefore);
			pickingTask.SetVirtualPallet(vp.Id);
			//pickingTask.VirtualPallet = virtualPallet;
			vp.PickingTasks.Add(pickingTask);
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(vp.PalletId);
			pickingTask.AddHistory(userId,sourcePallet.Id, sourcePallet.PalletNumber, issue.IssueNumber, PickingStatus.Available, PickingStatus.Allocated, 0);			
			return AddPickingTaskToIssueResult.Ok(pickingTask);
		}
		private static PickingTask CreatePickingTask(Issue issue, int quantity,
				VirtualPallet vp, Guid productId, DateOnly? bestBefore)
		{
			//return new()
			//{
			//	Issue = issue,
			//	IssueId = issue.Id,
			//	IssueNumber = issue.IssueNumber,	
			//	RequestedQuantity = quantity,
			//	PickingStatus = PickingStatus.Allocated,
			//	VirtualPallet = vp,
			//	ProductId = productId,
			//	BestBefore = bestBefore,
			//	PickingDay = DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)) //added new field
			//};
			return PickingTask.Create(vp.Id, issue.Id, quantity, PickingStatus.Allocated, productId, bestBefore, null,
				DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)), 0);
		}
	}
}