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
		//private readonly IProductRepo _productRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPalletRepo _palletRepo;
		public AddPickingTaskToIssueService(
			//IProductRepo productRepo,
			IPickingPalletRepo pickingPalletRepo, IPalletRepo palletRepo)
		{			
			//_productRepo = productRepo;
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
				var pickingTask = CreatePickingTask(issue,					
					taken, vp, productId, bestBefore);
				pickingTasks.Add(pickingTask);
				vp.PickingTasks.Add(pickingTask);
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
				}
			}
			if (quantity > 0)
			{
				return AddPickingTaskToIssueResult.Fail($"Nie ma więcej asortymentu {productId} - nie można utworzyć zadania pickingu.");
			}
			foreach (var pickingTask in pickingTasks)
			{
				pickingTask.AddHistory(userId, PickingStatus.Available, PickingStatus.Allocated, 0);				
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}

		//public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet virtualPallet, Issue issue, int productId, int quantity, DateOnly? bestBefore, string userId)
		public AddPickingTaskToIssueResult AddOnePickingTaskToIssue(VirtualPallet virtualPallet, Issue issue, Guid productId, int quantity, DateOnly? bestBefore, string userId)
		{
			//_ = await _productRepo.GetProductByIdAsync(productId) ?? throw new NotFoundProductException(productId);
			var pickingTask = CreatePickingTask(issue,				
				quantity, virtualPallet, productId, bestBefore);
			pickingTask.VirtualPallet = virtualPallet;
			virtualPallet.PickingTasks.Add(pickingTask);
			pickingTask.AddHistory(userId, PickingStatus.Available, PickingStatus.Allocated, 0);			
			return AddPickingTaskToIssueResult.Ok(pickingTask);
		}
		private static PickingTask CreatePickingTask(Issue issue, int quantity,
				VirtualPallet vp, Guid productId, DateOnly? bestBefore)
		{
			return new()
			{
				Issue = issue,
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,	
				RequestedQuantity = quantity,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = vp,
				ProductId = productId,
				BestBefore = bestBefore,
				PickingDay = DateOnly.FromDateTime(issue.IssueDateTimeSend.AddDays(-2)) //added new field
			};
		}
	}
}