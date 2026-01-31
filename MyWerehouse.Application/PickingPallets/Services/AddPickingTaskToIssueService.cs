using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public class AddPickingTaskToIssueService : IAddPickingTaskToIssueService
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;		
		private readonly IProductRepo _productRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IEventCollector _eventCollector;
		public AddPickingTaskToIssueService(IPickingTaskRepo pickingTaskRepo,			
			IProductRepo productRepo, IPickingPalletRepo pickingPalletRepo, IPalletRepo palletRepo,
			IEventCollector eventCollector)
		{
			_pickingTaskRepo = pickingTaskRepo;			
			_productRepo = productRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_palletRepo = palletRepo;
			_eventCollector = eventCollector;
		}
		public async Task<AddPickingTaskToIssueResult> AddPickingTaskToIssue(List<Pallet> pallets, List<VirtualPallet> virtualPallets, Issue issue, int productId, int rest, DateOnly? bestBefore, string userId)
		{		
			_ = await _productRepo.GetProductByIdAsync(productId) ?? throw new NotFoundProductException(productId);
			var pickingTasks = new List<PickingTask>();
			var quantity = rest;
			void AllocateFromVirtualPallet(VirtualPallet vp)
			{
				if (quantity <= 0 || vp.RemainingQuantity <= 0) return;
				var taken = Math.Min(quantity, vp.RemainingQuantity);
				var pickingTask = CreatePickingTask(issue, taken, vp, productId, bestBefore);
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
				var availablePallets = await _palletRepo.GetAvailablePallets(productId, bestBefore).ToListAsync();
				var usedPalletsId = pallets?
					.Select(p => p.Id)
					.ToHashSet()
					?? new HashSet<string>();
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
					palletToPicking.Status = PalletStatus.ToPicking;
					await _pickingPalletRepo.AddPalletToPickingAsync(virtualPallet);
					//_pickingPalletRepo.AddPalletToPicking(virtualPallet);
					_eventCollector.Add(new CreatePalletOperationNotification(
						palletToPicking.Id,	palletToPicking.LocationId,	ReasonMovement.Picking,
						userId,	PalletStatus.ToPicking,	null));
					AllocateFromVirtualPallet(virtualPallet);
				}
			}
			if (quantity > 0)
			{
				return AddPickingTaskToIssueResult.Fail($"Nie ma więcej asortymentu {productId} - nie można utworzyć zadania pickingu.");
			}
			foreach (var pickingTask in pickingTasks)
			{
				//_pickingTaskRepo.AddPickingTask(pickingTask);
				//history to remove in future
				_eventCollector.AddDeferred(() => new CreateHistoryPickingNotification(
					new HistoryDataPicking(
						pickingTask.Id, pickingTask.VirtualPallet.PalletId, pickingTask.IssueId,
						pickingTask.ProductId, pickingTask.RequestedQuantity, 0, PickingStatus.Allocated,
						pickingTask.PickingStatus, userId, DateTime.UtcNow)));
			}
			return AddPickingTaskToIssueResult.Ok(pickingTasks);
		}	

		public async Task<AddPickingTaskToIssueResult> AddOnePickingTaskToIssue(VirtualPallet virtualPallet, Issue issue, int productId, int quantity, DateOnly? bestBefore, string userId)
		{
			_ = await _productRepo.GetProductByIdAsync(productId) ?? throw new NotFoundProductException(productId);
			var pickingTask = CreatePickingTask(issue, quantity, virtualPallet, productId, bestBefore);
			pickingTask.VirtualPallet = virtualPallet;
			//_pickingTaskRepo.AddPickingTask(pickingTask);
			 virtualPallet.PickingTasks.Add(pickingTask);
			//history to remove in future
			_eventCollector.AddDeferred(() => new CreateHistoryPickingNotification(
				new HistoryDataPicking(
					pickingTask.Id, pickingTask.VirtualPallet.PalletId, pickingTask.IssueId,
					pickingTask.ProductId, pickingTask.RequestedQuantity, 0, PickingStatus.Allocated,
					pickingTask.PickingStatus,  userId, DateTime.UtcNow)));
			return AddPickingTaskToIssueResult.Ok(pickingTask);			
		}
	private static PickingTask CreatePickingTask(Issue issue, int quantity,
			VirtualPallet vp, int productId, DateOnly? bestBefore)
		{
			return new()
			{
				Issue = issue,
				RequestedQuantity = quantity,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = vp,
				ProductId = productId,
				BestBefore = bestBefore,
				PickingDay =DateOnly.FromDateTime( issue.IssueDateTimeSend.AddDays(-2)) //added new field
			};
		}
	}
}