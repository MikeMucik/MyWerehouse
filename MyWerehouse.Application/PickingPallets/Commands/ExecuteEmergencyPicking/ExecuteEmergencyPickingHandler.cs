using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteEmergencyPicking
{
	public class ExecuteEmergencyPickingHandler(IPalletRepo palletRepo,
		IPickingTaskRepo pickingTaskRepo,
		IPickingPalletRepo pickingPalletRepo,
		WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IProcessPickingActionService processPickingActionService) : IRequestHandler<ExecuteEmergencyPickingCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo = pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService = processPickingActionService;

		public async Task<AppResult<Unit>> Handle(ExecuteEmergencyPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
			{
				return AppResult<Unit>.Fail($"Paleta o numerze {request.PalletId} nie istnieje.", ErrorType.NotFound);
			}
			if (pallet.ProductsOnPallet.Count > 1)
			{
				return AppResult<Unit>.Fail("Zadania nie można zrealizować, paleta nie nadaje się do pobrań.", ErrorType.Conflict);
			}
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
			{
				return AppResult<Unit>.Fail($"Zamówienie o numerze {request.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}			
			var product = pallet.ProductsOnPallet.FirstOrDefault();
			if (product == null)
			{
				return AppResult<Unit>.Fail($"Paleta {request.PalletId} jest pusta.", ErrorType.Conflict);
			}
			// Oblicz, ile faktycznie można/trzeba skompletować
			var pickingTasksForIssue = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(request.IssueId, product.ProductId);
			if (pickingTasksForIssue == null) return AppResult<Unit>.Fail($"Zadanie do kompletacji nie istnieje", ErrorType.NotFound);
			var neededQuantity = pickingTasksForIssue.Where(a => a.PickingStatus == PickingStatus.Allocated).Sum(a => a.RequestedQuantity);
			var quantityToPick = Math.Min(neededQuantity, product.Quantity);
			if (quantityToPick <= 0)
			{
				return AppResult<Unit>.Fail("Brak zapotrzebowania na ten produkt dla wybranego zlecenia.", ErrorType.Conflict);
			}
			VirtualPallet virtualPallet;
			var vpId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletId);
			if (vpId != Guid.Empty)
			{
				virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(vpId);
			}
			// dodanie do palety virtualPallet - można obęjść przez zmianę statusu, osobna akcja - do przemyślenia
			else
			{
				pallet.ChangeStatus(PalletStatus.ToPicking);//jeśli nie jest można zmienić jeśli zmieni się podejście biznesowe - najpierw krok że zmiana statusu - teraz bez
				virtualPallet = VirtualPallet.Create(pallet.Id, pallet.ProductsOnPallet.Single(p => p.PalletId == pallet.Id).Quantity, pallet.LocationId);
				_pickingPalletRepo.AddPalletToPicking(virtualPallet); 
			}
			await ReduceAllocation(issue, product.ProductId, quantityToPick, request.UserId);
			var newPickingTaskInfo = await _addPickingTaskToIssueService.AddOnePickingTaskToIssue(virtualPallet, issue, product.ProductId, quantityToPick, product.BestBefore, request.UserId);
			
			var newPickingTask = newPickingTaskInfo.OnePickingTask;
			await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, quantityToPick, request.UserId, newPickingTask, PickingCompletion.Full, request.RampNumber);
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Towar dołączono do zlecenia");
		}
		
		private async Task ReduceAllocation(Issue issue, Guid productId, int quantity, string userId)
		{
			var pickingTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(issue.Id, productId);
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
					pickingTask.Cancel(userId, issue.IssueNumber);
				}
			}
		}
	}
}
