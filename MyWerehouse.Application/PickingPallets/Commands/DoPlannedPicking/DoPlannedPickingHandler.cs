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

namespace MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking
{
	public class DoPlannedPickingHandler : IRequestHandler<DoPlannedPickingCommand, AppResult<Unit>>
	{
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IIssueRepo _issueRepo;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService;
		private readonly IProcessPickingActionService _processPickingActionService;
		public DoPlannedPickingHandler(IPickingTaskRepo pickingTaskRepo,
			IPalletRepo palletRepo,
			IIssueRepo issueRepo,
			IPickingPalletRepo pickingPalletRepo,
			WerehouseDbContext werehouseDbContext,
			IAddPickingTaskToIssueService addPickingTaskToIssueService,
			IProcessPickingActionService processPickingActionService)
		{
			_pickingTaskRepo = pickingTaskRepo;
			_palletRepo = palletRepo;
			_issueRepo = issueRepo;
			_pickingPalletRepo = pickingPalletRepo;
			_werehouseDbContext = werehouseDbContext;
			_addPickingTaskToIssueService = addPickingTaskToIssueService;
			_processPickingActionService = processPickingActionService;
		}
		public async Task<AppResult<Unit>> Handle(DoPlannedPickingCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var newPickingTask = new PickingTask();
				var pickingTaskToChange = await _pickingTaskRepo.GetPickingTaskAsync(request.PickingTaskDTO.Id);
				var virtualPallet = await _pickingPalletRepo.GetVirtualPalletByIdAsync(pickingTaskToChange.VirtualPalletId);
				var issueId = pickingTaskToChange.IssueId;
				var issue = await _issueRepo.GetIssueByIdAsync(issueId);
				if (issue == null)
				{
					return AppResult<Unit>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);
				}
				//if
				var sourcePallet = await _palletRepo.GetPalletByIdAsync(request.PickingTaskDTO.SourcePalletId.Value);
				if (sourcePallet == null) return AppResult<Unit>.Fail($"Paleta o numerze {request.PickingTaskDTO.SourcePalletId} nie istnieje.", ErrorType.NotFound);
				if (issue.IssueStatus == IssueStatus.Pending) { issue.IssueStatus = IssueStatus.InProgress; }
				var neededQuantity = request.PickingTaskDTO.RequestedQuantity;
				var pickedQuantity = request.PickingTaskDTO.PickedQuantity;
				var completion = PickingCompletion.Full;
				if (pickedQuantity <= 0 || pickedQuantity > neededQuantity)
				{
					await transaction.RollbackAsync(ct);
					return AppResult<Unit>.Fail("Operacja nie dozwolona", ErrorType.Conflict);//Technical
				}
				if (neededQuantity > pickedQuantity)
				{
					completion = PickingCompletion.Partial;
				}

				await _processPickingActionService.ProcessPicking(sourcePallet, issue, request.PickingTaskDTO.ProductId,
						request.PickingTaskDTO.PickedQuantity, request.UserId, pickingTaskToChange, completion, request.PickingTaskDTO.RampNumber);
				pickingTaskToChange.AddHistory(request.UserId, PickingStatus.Allocated, pickingTaskToChange.PickingStatus, request.PickingTaskDTO.PickedQuantity);
				if (neededQuantity == pickedQuantity)
				{
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					return AppResult<Unit>.Success(Unit.Value, "Towar dołączono do zlecenia");
				}
				else
				{
					//pallet lock with non-conformity 
					sourcePallet.AddHistory(PalletStatus.OnHold, ReasonMovement.Correction, request.UserId);
					var newQuantityToPickingTask = neededQuantity - pickedQuantity;
					var newVirtualPallet = await _addPickingTaskToIssueService.AddPickingTaskToIssue(null, new List<VirtualPallet>(),
						issue, pickingTaskToChange.ProductId, newQuantityToPickingTask, pickingTaskToChange.BestBefore, request.UserId);

					if (newVirtualPallet.Success == false)
					{
						await transaction.RollbackAsync(ct);
						return AppResult<Unit>.Fail(newVirtualPallet.Message, ErrorType.Conflict);
					}
					await _werehouseDbContext.SaveChangesAsync(ct);
					await transaction.CommitAsync(ct);
					return AppResult<Unit>.Success(Unit.Value, "Towar dołączono do zlecenia, wykonano nie pełne zadanie kompletacyjne, stworzono dodatkowe zadanie do pickingu. Poproś o nowe palety do kompletacji.");
				}
			}
			//catch (NotFoundPalletException pnfEx)
			//{
			//	await transaction.RollbackAsync(ct);
			//	return AppResult<PickingResult>.Fail(pnfEx.Message);
			//}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");				
				//return AppResult<PickingResult>.Fail("Wystąpił nieoczekiwany błąd. Zmiany zostały cofnięte.", ErrorType.Conflict);
				throw;
			}
		}
	}
}
