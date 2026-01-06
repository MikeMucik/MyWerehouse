using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.VerifyIssueToLoad
{
	public class VerifyIssueToLoadHandler :IRequestHandler<VerifyIssueToLoadCommand, IssueResult>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IMediator _mediator;
		public VerifyIssueToLoadHandler(IIssueRepo issueRepo,
			WerehouseDbContext werehouseDbContext,
			IMediator mediator)
		{
			_issueRepo = issueRepo;
			_werehouseDbContext = werehouseDbContext;
			_mediator = mediator;	
		}
		public async Task<IssueResult> Handle(VerifyIssueToLoadCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var issue = await _issueRepo.GetIssueByIdAsync(request.IssueId)
						?? throw new NotFoundIssueException(request.IssueId);
				List<INotification> notificationList = [];
				issue.IssueStatus = IssueStatus.ConfirmedToLoad;
				foreach (var pallet in issue.Pallets)
				{
					pallet.Status = PalletStatus.ToIssue;
					notificationList.Add(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId, ReasonMovement.ToLoad, request.UserId, PalletStatus.ToIssue, null));
				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				await _mediator.Publish(new CreateHistoryIssueNotification(request.IssueId, request.UserId), ct);//
				foreach (var p in notificationList)
				{
					await _mediator.Publish(p, ct);
				}
				return IssueResult.Ok("Wydanie zatwierdzono.", request.IssueId);
			}
			catch (NotFoundIssueException ei)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ei.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas zatwierdzania zlecenia.", ex);
			}
		}
	}
}
