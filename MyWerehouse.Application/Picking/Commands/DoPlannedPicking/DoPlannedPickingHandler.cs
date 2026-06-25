using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Picking.Commands.DoPlannedPicking
{
	public class DoPlannedPickingHandler(IPickingTaskRepo pickingTaskRepo,
		IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IProcessPickingActionService processPickingActionService)
		: IRequestHandler<DoPlannedPickingCommand, AppResult<ProcessPickingActionResult>>
	{
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService = processPickingActionService;

		public async Task<AppResult<ProcessPickingActionResult>> Handle(DoPlannedPickingCommand request, CancellationToken ct)
		{
			var pickingTaskToChange = await _pickingTaskRepo.GetPickingTaskAsync(request.PickingTaskDTO.Id);
			var issueId = pickingTaskToChange.IssueId;
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			}
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.PickingTaskDTO.SourcePalletId.Value);
			if (sourcePallet == null) return AppResult<ProcessPickingActionResult>.Fail($"Paleta o numerze {request.PickingTaskDTO.SourcePalletId} nie istnieje.", ErrorType.NotFound);
			if (request.PickingTaskDTO.PickedQuantity <= 0)
			{
				return AppResult<ProcessPickingActionResult>.Fail("Nie możesz pobrać ujemnej wartości.", ErrorType.Conflict);
			}
			var neededQuantity = request.PickingTaskDTO.RequestedQuantity;
			var pickedQuantity = request.PickingTaskDTO.PickedQuantity;
			var completion = PickingCompletion.Full;
			if (pickedQuantity <= 0 || pickedQuantity > neededQuantity)
			{
				return AppResult<ProcessPickingActionResult>.Fail("Ilosć musi być większa od zera i mniejsza od zapotrzebowania", ErrorType.Conflict);//Technical
			}
			if (neededQuantity > pickedQuantity)
			{
				completion = PickingCompletion.Partial;
			}
			if (issue.IssueStatus == IssueStatus.Pending)
			{
				issue.ChangeStatus(IssueStatus.InProgress);
			}
			var resultProccesPicking = await _processPickingActionService.ProcessPicking(sourcePallet, issue, request.PickingTaskDTO.ProductId,
						request.PickingTaskDTO.PickedQuantity, request.UserId, pickingTaskToChange, completion, request.PickingTaskDTO.RampNumber);
			if (!resultProccesPicking.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(resultProccesPicking.Message, ErrorType.Conflict);
			}
			if (neededQuantity == pickedQuantity)
			{
				await _werehouseDbContext.SaveChangesAsync(ct);
				return AppResult<ProcessPickingActionResult>.Success(resultProccesPicking);
			}
			else
			{
				var oldListVirtualPallet = new List<VirtualPallet>(); //TODO pobierz dostępne virtualPallet lub usuń opcję czy wyszukiwać inne palety do kompletacji, nie powinno być ale życie sobie
				var newQuantityToPickingTask = neededQuantity - pickedQuantity;
				var newVirtualPallet = await _addPickingTaskToIssueService.AddPickingTaskToIssue(null, oldListVirtualPallet,
					issue, pickingTaskToChange.ProductId, newQuantityToPickingTask, pickingTaskToChange.BestBefore, request.UserId);
				var partialResult = new ProcessPickingActionResult
				{
					Success = true,
					NewPalletCreated = resultProccesPicking.NewPalletCreated,
					PalletId = resultProccesPicking.PalletId,
					PalletNumber = resultProccesPicking.PalletNumber,
					RequestedQuantity = neededQuantity,
					PickedQuantity = pickedQuantity,
					MissingQuantity = newQuantityToPickingTask,
				};
				if (newVirtualPallet.Success == false)
				{
					issue.ChangeStatus(IssueStatus.PickingShortage);
					sourcePallet.ChangeStatus(PalletStatus.OnHold);
					sourcePallet.AddHistory(ReasonForPallet.Correction, request.UserId, sourcePallet.Location.ToSnapshot());
					await _werehouseDbContext.SaveChangesAsync(ct);

					partialResult.Message =
					$"Wykonano częściową kompletację. Pobrano {pickedQuantity} z {neededQuantity}. " +
					$"Brakuje {newQuantityToPickingTask}. Brak towaru na magazynie. " +
					$"Zlecenie zmieniono na status {IssueStatus.PickingShortage}.";

					return AppResult<ProcessPickingActionResult>.Success(
						partialResult,
						newVirtualPallet.Message);
				}
				//pallet lock with non-conformity
				sourcePallet.ChangeStatus(PalletStatus.OnHold);
				sourcePallet.AddHistory(ReasonForPallet.Correction, request.UserId, sourcePallet.Location.ToSnapshot());
				await _werehouseDbContext.SaveChangesAsync(ct);
				partialResult.Message =
				$"Wykonano częściową kompletację. Pobrano {pickedQuantity} z {neededQuantity}. " +
				$"Brakuje {newQuantityToPickingTask}. Utworzono dodatkowe zadanie do pickingu.";

				return AppResult<ProcessPickingActionResult>.Success(
					partialResult,
					"Wykonano niepełne zadanie kompletacyjne.Poproś o nowe palety źródłowe do kompletacji.");
			}
		}
	}
}
