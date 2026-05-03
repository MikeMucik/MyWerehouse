using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking
{
	public class DoPlannedPickingHandler(IPickingTaskRepo pickingTaskRepo,
		IPalletRepo palletRepo,
		IIssueRepo issueRepo,
		WerehouseDbContext werehouseDbContext,
		IAddPickingTaskToIssueService addPickingTaskToIssueService,
		IProcessPickingActionService processPickingActionService) : IRequestHandler<DoPlannedPickingCommand, AppResult<Unit>>
	{
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService = processPickingActionService;

		public async Task<AppResult<Unit>> Handle(DoPlannedPickingCommand request, CancellationToken ct)
		{
			var pickingTaskToChange = await _pickingTaskRepo.GetPickingTaskAsync(request.PickingTaskDTO.Id);
			var issueId = pickingTaskToChange.IssueId;
			var issue = await _issueRepo.GetIssueByIdAsync(issueId);
			if (issue == null)
			{
				return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
			}
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.PickingTaskDTO.SourcePalletId.Value);
			if (sourcePallet == null) return AppResult<Unit>.Fail($"Paleta o numerze {request.PickingTaskDTO.SourcePalletId} nie istnieje.", ErrorType.NotFound);
			if (request.PickingTaskDTO.PickedQuantity <= 0)
			{
				return AppResult<Unit>.Fail("Nie możesz pobrać ujemnej wartości.", ErrorType.Conflict);
			}
			var neededQuantity = request.PickingTaskDTO.RequestedQuantity;
			var pickedQuantity = request.PickingTaskDTO.PickedQuantity;
			var completion = PickingCompletion.Full;
			if (pickedQuantity <= 0 || pickedQuantity > neededQuantity)
			{
				return AppResult<Unit>.Fail("Ilosć musi być większa od zera i mniejsza od zapotrzebowania", ErrorType.Conflict);//Technical
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
				return AppResult<Unit>.Fail(resultProccesPicking.Message, ErrorType.Conflict);
			}
			if (neededQuantity == pickedQuantity)
			{
				await _werehouseDbContext.SaveChangesAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Towar dołączono do zlecenia");
			}
			else
			{
				var oldListVirtualPallet = new List<VirtualPallet>(); //TODO pobierz dostępne virtualPallet
				var newQuantityToPickingTask = neededQuantity - pickedQuantity;
				var newVirtualPallet = await _addPickingTaskToIssueService.AddPickingTaskToIssue(null, oldListVirtualPallet,
					issue, pickingTaskToChange.ProductId, newQuantityToPickingTask, pickingTaskToChange.BestBefore, request.UserId);

				if (newVirtualPallet.Success == false)
				{
					return AppResult<Unit>.Fail(newVirtualPallet.Message, ErrorType.Conflict);
				}
				//pallet lock with non-conformity
				sourcePallet.ChangeStatus(PalletStatus.OnHold);
				sourcePallet.AddHistory(ReasonMovement.Correction, request.UserId, sourcePallet.Location.ToSnapshot());
				await _werehouseDbContext.SaveChangesAsync(ct);
				return AppResult<Unit>.Success(Unit.Value, "Towar dołączono do zlecenia, wykonano nie pełne zadanie kompletacyjne, stworzono dodatkowe zadanie do pickingu. Poproś o nowe palety źródło do kompletacji.");
			}
		}
	}
}
