using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking
{
	public class ExecuteEmergencyPickingHandler(IPalletRepo palletRepo,
		IPickingTaskRepo pickingTaskRepo,
		IVirtualPalletRepo virtualPalletRepo,
		WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IProcessPickingActionService processPickingActionService) : IRequestHandler<ExecuteEmergencyPickingCommand, AppResult<ProcessPickingActionResult>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService = processPickingActionService;

		public async Task<AppResult<ProcessPickingActionResult>> Handle(ExecuteEmergencyPickingCommand request, CancellationToken ct)
		{
			
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
			}
			if (pallet.ProductsOnPallet.Count > 1)
			{
				return AppResult<ProcessPickingActionResult>.Fail("Zadania nie można zrealizować, paleta nie nadaje się do pobrań.", ErrorType.Conflict);
			}
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}			
			var palletItem = pallet.ProductsOnPallet.SingleOrDefault();
			if (palletItem == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Paleta {request.PalletId} jest pusta.", ErrorType.Conflict);
			}
			// Oblicz, ile faktycznie można/trzeba skompletować
			var pickingTasksForIssue = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(request.IssueId, palletItem.ProductId);
			if (pickingTasksForIssue == null) return AppResult<ProcessPickingActionResult>.Fail($"Zadanie do kompletacji nie istnieje", ErrorType.NotFound);
			var neededQuantity = pickingTasksForIssue.Where(a => a.PickingStatus == PickingStatus.Allocated).Sum(a => a.RequestedQuantity);
			// Emergency picking obsługuje tylko brakującą końcówkę ilości z aktywnej palety pickingowej.
			var quantityToPick = Math.Min(neededQuantity, palletItem.Quantity);
			if (quantityToPick <= 0)
			{
				return AppResult<ProcessPickingActionResult>.Fail("Brak zapotrzebowania na ten produkt dla wybranego zlecenia.", ErrorType.Conflict);
			}
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(request.PalletId);
			// W obecnym flow paleta trafia bezpośrednio do ToPicking; osobna akcja zmiany statusu może być dodana później.
			if (virtualPallet == null)			
			{
				pallet.ChangeStatus(PalletStatus.ToPicking);
				pallet.AssignToPicking(request.UserId, pallet.Location.ToSnapshot());
				virtualPallet = VirtualPallet.Create(pallet.Id, palletItem.Quantity, pallet.LocationId);
				_virtualPalletRepo.AddPalletToPicking(virtualPallet); 
			}

			await ReduceAllocation(issue.Id, palletItem.ProductId, quantityToPick, request.UserId);
			var newPickingTaskInfo = await _addPickingTaskToIssueService.AddOnePickingTaskToIssue(virtualPallet, issue, palletItem.ProductId, quantityToPick, palletItem.BestBefore, request.UserId);
			if (!newPickingTaskInfo.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(
					newPickingTaskInfo.Message,
					ErrorType.Conflict);
			}
			var newPickingTask = newPickingTaskInfo.PickingTask.Single();
			var resultProccessPicking = await _processPickingActionService.ProcessPicking(pallet, issue, palletItem.ProductId, quantityToPick, request.UserId, newPickingTask, PickingCompletion.Full, request.RampNumber);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<ProcessPickingActionResult>.Success(resultProccessPicking, "Towar dołączono do zlecenia");
		}
		
		private async Task ReduceAllocation(Guid issueId, Guid productId, int quantity, string userId)
		{
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issueId, productId);
			foreach (var pickingTask in pickingTasks)
			{
				if (quantity <= 0) break;
				if (pickingTask.RequestedQuantity > quantity)
				{
					pickingTask.ReduceQuantity(quantity, userId);
					quantity = 0;
				}
				else
				{
					quantity -= pickingTask.RequestedQuantity;
					pickingTask.Cancel(userId);
				}
			}
		}
	}
}
