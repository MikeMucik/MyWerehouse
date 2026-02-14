using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Events.CreateHistoryIssue
{
	public class CreateHistoryIssueHandler :INotificationHandler<AddHistoryForIssueNotification>
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IHistoryIssueRepo _historyIssueRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public CreateHistoryIssueHandler(
			IIssueRepo issueRepo,
			IHistoryIssueRepo historyIssueRepo,
			WerehouseDbContext werehouseDbContext
			)
		{
			_issueRepo = issueRepo;
			_historyIssueRepo = historyIssueRepo;		
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task Handle(AddHistoryForIssueNotification request, CancellationToken cancellationToken)
		{
			var issue = await _issueRepo.GetIssueByIdWithPalletAndItemsAsync(request.IssueId, cancellationToken)
				?? throw  new NotFoundIssueException(request.IssueId);

			var details = issue.Pallets != null && issue.Pallets.Count > 0 ?

				issue.Pallets.Select(p => new HistoryIssueDetail
				{
					PalletId = p.Id,
					LocationId = p.LocationId,
					LocationSnapShot = $"{p.Location.Bay}-{p.Location.Aisle}-{p.Location.Position}-{p.Location.Height}"
				}).ToList() : new List<HistoryIssueDetail>();

			var items = issue.IssueItems.Select(p => new HistoryIssueItems
			{
				ProductId = p.ProductId,
				Quantity = p.Quantity,
				BestBefore = p.BestBefore,
			}).ToList();

			var history = new HistoryIssue
			{
				IssueId = issue.Id,
				StatusAfter = issue.IssueStatus,
				PerformedBy = request.PerformedBy??issue.PerformedBy,
				Details = details.ToList(),
				Items = items,
				DateTime = DateTime.UtcNow,
			};
			await _historyIssueRepo.AddHistoryIssueAsync(history, cancellationToken);
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
		}
	}
}
