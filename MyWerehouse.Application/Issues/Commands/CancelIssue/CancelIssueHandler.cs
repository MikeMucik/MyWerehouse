using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Services;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.CancelIssue
{
	public class CancelIssueHandler(IIssueRepo issueRepo,
		IPickingTaskRepo pickingTaskRepo,
		IVirtualPalletRepo virtualPalletRepo,
		WerehouseDbContext werehouseDbContext,
		ICreateReversePickingService createReversePickingService,
		IDateTimeProvider dateTimeProvider,
		IPickingDomainService pickingDomainService
			) : IRequestHandler<CancelIssueCommand, AppResult<Unit>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly ICreateReversePickingService _createReversePickingService = createReversePickingService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		private readonly IPickingDomainService _pickingDomainService = pickingDomainService;
		public async Task<AppResult<Unit>> Handle(CancelIssueCommand request, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId);
			if (issue == null)
				return AppResult<Unit>.Fail("Issue was not found.");
			issue.EnsureCanBeCancelled();
			var palletsToReversepicking = issue.ReturnPickingPallets();		
			foreach (var p in palletsToReversepicking)
			{
				var resultReverse = await _createReversePickingService.CreateReversePicking(p.Id, request.UserId);
				if (!resultReverse.Success) return AppResult<Unit>.Fail(resultReverse.Message);
			}				
			var virtualPallets = await _issueRepo.GetVirtualPalletsAsync(request.IssueId);
			var result = _pickingDomainService.ListVirtualPalletPickingTaskToCancel(virtualPallets, issue.Id, request.UserId, now);
			foreach (var virtualPalletToCancel in result.Item1)
			{
				_virtualPalletRepo.DeleteVirtualPalletPicking(virtualPalletToCancel);
			}
			foreach (var pickingTaksToCancel in result.Item2)
			{
				_pickingTaskRepo.DeletePickingTask(pickingTaksToCancel);
			}
			var pickinkHandTasksToCancel = await _pickingTaskRepo.GetHandPickingTask(issue.Id);
			foreach (var handTask in pickinkHandTasksToCancel)
			{
				handTask.Cancel(request.UserId, now);
				_pickingTaskRepo.DeletePickingTask(handTask);
			}
			issue.DetachPallets(request.UserId);
			issue.Cancel(request.UserId);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, $"Issue {request.IssueId} was cancelled.");
		}
	}
}
