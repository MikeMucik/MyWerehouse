using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Services;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Picking.Commands.ExecuteHandPicking
{
	public class ExecuteHandPickingHandler(IPalletRepo palletRepo,
		IVirtualPalletRepo virtualPalletRepo,
		WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IExecuteProcessPickingService processPickingActionService,
		IPickingDomainService pickingDomainService,
		IDateTimeProvider dateTimeProvider,
		IPickingTaskRepo pickingTaskRepo,
		IAddPickingTaskToIssueService addPickingTaskToIssueService
			) : IRequestHandler<ExecuteHandPickingCommand, AppResult<ProcessPickingActionResult>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IExecuteProcessPickingService _processPickingActionService = processPickingActionService;
		private readonly IPickingDomainService _pickingDomainService = pickingDomainService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IAddPickingTaskToIssueService _addPickingTaskToIssueService = addPickingTaskToIssueService;


		public async Task<AppResult<ProcessPickingActionResult>> Handle(ExecuteHandPickingCommand command, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var issue = await _issueRepo.GetIssueByIdAsync(command.IssueId);
			if (issue == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Issue {command.IssueId} was not found.");
			}

			var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletIdSource);// W hand picking paleta źródłowa jest wskazywana ręcznie przez biuro.
			if (pallet == null)
			{
				return AppResult<ProcessPickingActionResult>.Fail($"Pallet {command.PalletIdSource} does not exist.");
			}
			var palletItem = pallet.EnsureCanBeUsedForPicking();

			var tasks = await _pickingTaskRepo.GetPickingTasksByIssueIdProductIdAsync(command.IssueId, palletItem.ProductId);

			var pickingHandTask = _pickingDomainService.GetSingleHandPickingTask(tasks, command.IssueId, palletItem.ProductId);//sprawdzenie czy został ainicjalizowana ręczna kompletacja

			pickingHandTask.BeginExecuteHandPicking(command.PickedQuantity);
			pallet.IsCorrectDate(pickingHandTask.BestBefore);
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(command.PalletIdSource);
			if (virtualPallet == null)
			{
				virtualPallet = VirtualPallet.Create(pallet.Id, palletItem.Quantity, pallet.LocationId, now);
				pallet.AssignToPicking(command.UserId, pallet.Location.ToSnapshot());
				_virtualPalletRepo.AddPalletToPicking(virtualPallet);
			}
			var availableQuantity = virtualPallet.RemainingQuantity;// ?? product.Quantity;//wydaje mi się że to zakomentowane tu zbędne
			if (command.PickedQuantity > availableQuantity)
			{
				return AppResult<ProcessPickingActionResult>.Fail("The pallet contains less product than the requested picking quantity.", ErrorType.Conflict);
			}

			var newPickingTaskInfo = await _addPickingTaskToIssueService.AddOnePickingTaskToIssue(virtualPallet, issue, palletItem.ProductId, command.PickedQuantity, pickingHandTask.BestBefore, command.UserId);
			if (!newPickingTaskInfo.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(
					newPickingTaskInfo.Message,
					ErrorType.Conflict);
			}
			var newPickingTask = newPickingTaskInfo.PickingTask.Single();
			var resultProcessPicking = await _processPickingActionService.ExecuteProcessPicking(pallet, newPickingTask, command.PickedQuantity, command.UserId, command.RampNumber);
			if (!resultProcessPicking.Success)
			{
				return AppResult<ProcessPickingActionResult>.Fail(resultProcessPicking.Message, ErrorType.Conflict);
			}
			pickingHandTask.CompleteHandPicking(command.PickedQuantity);
			issue.CompletePicking();
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<ProcessPickingActionResult>.Success(resultProcessPicking, "Product was added to the issue.");
		}
	}
}
