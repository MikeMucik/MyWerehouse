using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteCorrectedPicking
{
	public class ExecuteCorrectedPickingHandler : IRequestHandler<ExecuteCorrectedPickingCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService;
		private readonly IReduceAllocationService _reduceAllocationService;
		public ExecuteCorrectedPickingHandler(IPalletRepo palletRepo,
			IPickingTaskRepo pickingTaskRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IAddPickingTaskToIssueService addPickingTaskToIssueService,
			IProcessPickingActionService processPickingActionService,
			IReduceAllocationService reduceAllocationService)
		{
			_palletRepo = palletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
			_processPickingActionService = processPickingActionService;
			_reduceAllocationService = reduceAllocationService;
		}
		public async Task<AppResult<Unit>> Handle(ExecuteCorrectedPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
				if (pallet  == null)
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
				if (vpId != 0)
				{
					int virtualPalletId = vpId;
					virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(virtualPalletId);
				}
				// dodanie do palety virtualPallet - można obęjść przez zmianę statusu, osobna akcja
				else
				{
					pallet.ChangeStatus(PalletStatus.ToPicking);
					virtualPallet = new VirtualPallet
					{
						Pallet = pallet,
						PalletId = pallet.Id,
						DateMoved = DateTime.UtcNow,
						LocationId = pallet.LocationId,
						InitialPalletQuantity = pallet.ProductsOnPallet.Single(p => p.PalletId == pallet.Id).Quantity,//zakładam że jest jeden towar
						PickingTasks = new List<PickingTask>()
					};
					_pickingPalletRepo.AddPalletToPicking(virtualPallet);  // Dodaj do repo
				}
				await _reduceAllocationService.ReduceAllocation(issue, product.ProductId, quantityToPick, request.UserId);
				var newPickingTaskInfo =await _addPickingTaskToIssueService.AddOnePickingTaskToIssue(virtualPallet, issue, product.ProductId, quantityToPick, product.BestBefore, request.UserId);
				var newPickingTask = newPickingTaskInfo.OnePickingTask;
				await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, quantityToPick, request.UserId, newPickingTask, PickingCompletion.Full, request.RampNumber);

				await _werehouseDbContext.SaveChangesAsync(ct);

				await transaction.CommitAsync(ct);

				return AppResult<Unit>.Success(Unit.Value,"Towar dołączono do zlecenia");
			}			
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				return AppResult<Unit>.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.", ErrorType.Technical);
			}
		}
	}
}
