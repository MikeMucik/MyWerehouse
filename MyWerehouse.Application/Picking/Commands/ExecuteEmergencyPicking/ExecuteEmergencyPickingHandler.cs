using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.PalletExceptions;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Services;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking
{
	public class ExecuteEmergencyPickingHandler(IPalletRepo palletRepo,
		IPickingTaskRepo pickingTaskRepo,
		IVirtualPalletRepo virtualPalletRepo,
		WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IExecuteProcessPickingService processPickingActionService,
		IDateTimeProvider dateTimeProvider,
		IPickingDomainService pickingDomainService) : IRequestHandler<ExecuteEmergencyPickingCommand, AppResult<ProcessPickingActionResult>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IExecuteProcessPickingService _processPickingActionService = processPickingActionService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		private readonly IPickingDomainService _pickingDomainService = pickingDomainService;
		public async Task<AppResult<ProcessPickingActionResult>> Handle(ExecuteEmergencyPickingCommand request, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId);
			if (pallet == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Pallet {request.PalletId} does not exist.");
			}
			var palletItem = pallet.EnsureCanBeUsedForPicking();
						
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Issue {request.IssueId} was not found.");
			}
			issue.StartEmergencyPicking();
			
			// Oblicz, ile faktycznie można/trzeba skompletować
			var pickingTasksForIssue = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(request.IssueId, palletItem.ProductId);
			if (pickingTasksForIssue.Count == 0) return AppResult<ProcessPickingActionResult>.Fail($"Picking task does not exist.");
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(request.PalletId);
			var availableQuantity = virtualPallet?.RemainingQuantity ?? palletItem.Quantity;
			if (availableQuantity == 0)
			{
				throw new InsufficientQuantityDomainException(pallet.Id, pallet.PalletNumber);
			}
			var reallocation = _pickingDomainService.ReallocateForEmergencyPicking(pickingTasksForIssue,
				availableQuantity, request.UserId, now, request.IssueId, palletItem.ProductId, pallet.Id, pallet.PalletNumber);
			var quantityToPick = reallocation.QuantityToPick;
			//czy paleta ma dobrą BB
			pallet.IsCorrectDate(reallocation.BestBefore);
			
			// W obecnym flow paleta trafia bezpośrednio do ToPicking; osobna akcja zmiany statusu może być dodana później.
			if (virtualPallet == null)
			{
				pallet.AssignToPicking(request.UserId, pallet.Location.ToSnapshot());
				virtualPallet = VirtualPallet.Create(pallet.Id, palletItem.Quantity, pallet.LocationId, now);
				_virtualPalletRepo.AddPalletToPicking(virtualPallet);
			}
			
			var newPickingTaskInfo = await _addPickingTaskToIssueService.AddOnePickingTaskToIssue(virtualPallet, issue, palletItem.ProductId, quantityToPick, reallocation.BestBefore, request.UserId);
			if (!newPickingTaskInfo.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(
					newPickingTaskInfo.Message,
					ErrorType.Conflict);
			}
			var newPickingTask = newPickingTaskInfo.PickingTask.Single();
			var resultProccessPicking = await _processPickingActionService.ExecuteProcessPicking(pallet, newPickingTask,
				quantityToPick, request.UserId, request.RampNumber);
			if (!resultProccessPicking.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(
					resultProccessPicking.Message,
					ErrorType.Conflict);
			}
			issue.CompletePicking();
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<ProcessPickingActionResult>.Success(resultProccessPicking, "Product was added to the issue.");
		}
	}
}
