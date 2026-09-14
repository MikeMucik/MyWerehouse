using MediatR;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Picking.Commands.DoPlannedPicking
{
	public class DoPlannedPickingHandler(IPickingTaskRepo pickingTaskRepo,
		IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		IUnitOfWork unitOfWork,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IExecuteProcessPickingService processPickingActionService)
		: IRequestHandler<DoPlannedPickingCommand, AppResult<ProcessPickingActionResult>>
	{
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IExecuteProcessPickingService _processPickingActionService = processPickingActionService;
		public async Task<AppResult<ProcessPickingActionResult>> Handle(DoPlannedPickingCommand request, CancellationToken ct)
		{
			var pickingTaskToChange = await _pickingTaskRepo.GetPickingTaskAsync(request.PickingTaskId, ct);
			if (pickingTaskToChange == null)
				return AppResult<ProcessPickingActionResult>.Fail("Picking task was not found.");
			var issueId = pickingTaskToChange.IssueId;
			var issue = await _issueRepo.GetIssueByIdAsync(issueId, ct);
			if (issue == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail("Issue was not found.");
			}
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.SourcePalletId, ct);
			if (sourcePallet == null)
				return AppResult<ProcessPickingActionResult>.Fail($"Pallet {request.SourcePalletId} does not exist.");
			pickingTaskToChange.EnsureSourcePallet(sourcePallet.Id);
			var neededQuantity = pickingTaskToChange.RequestedQuantity;
			var pickedQuantity = request.PickedQuantity;
			var resultProccesPicking = await _processPickingActionService.ExecuteProcessPicking(sourcePallet,pickingTaskToChange, request.PickedQuantity, request.UserId,  request.RampNumber, ct);
			if (!resultProccesPicking.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(resultProccesPicking.Message, ErrorType.Conflict);
			}
			if (neededQuantity == pickedQuantity)
			{
				issue.CompletePicking();
				await _unitOfWork.SaveChangesAsync(ct);
				return AppResult<ProcessPickingActionResult>.Success(resultProccesPicking);
			}
			else//pickedQuantity<neededQuantity
			{
				var newQuantityToPickingTask = neededQuantity - pickedQuantity;
				var newVirtualPallet = await _addPickingTaskToIssueService.AddPickingTasksToIssue(null, null,
					issue, pickingTaskToChange.ProductId, newQuantityToPickingTask, pickingTaskToChange.BestBefore, request.UserId, ct);
				issue.CompletePickingPlanned(newVirtualPallet.Success, sourcePallet, request.UserId);
				if (newVirtualPallet.Success == false)
				{
					await _unitOfWork.SaveChangesAsync(ct);

					resultProccesPicking.Message =
					$"Partial picking completed. Picked {pickedQuantity} of {neededQuantity}. " +
					$"Missing quantity: {newQuantityToPickingTask}. No stock is available. " +
					"Create a new issue for the missing quantity when stock becomes available. " +
					$"The issue status was changed to {IssueStatus.PickingShortage}.";

					return AppResult<ProcessPickingActionResult>.Success(
						resultProccesPicking,
						newVirtualPallet.Message);
				}
				//pallet lock with non-conformity
				await _unitOfWork.SaveChangesAsync(ct);
				resultProccesPicking.Message =
				$"Partial picking completed. Picked {pickedQuantity} of {neededQuantity}. " +
				$"Missing quantity: {newQuantityToPickingTask}. An additional picking task was created.";

				return AppResult<ProcessPickingActionResult>.Success(
					resultProccesPicking,
					"Picking task was completed partially. Request new source pallets to continue picking.");
			}
		}
	}
}
