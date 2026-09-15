using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Events;

namespace MyWerehouse.Application.Issues.Events.CreateHistoryIssue
{
	public class CreateHistoryIssueHandler(IHistoryIssueRepo historyIssueRepo, IDateTimeProvider dateTimeProvider)
		: INotificationHandler<DomainEventNotification<AddHistoryForIssueNotification>>
	{
		private readonly IHistoryIssueRepo _historyIssueRepo = historyIssueRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public Task Handle(DomainEventNotification<AddHistoryForIssueNotification> request, CancellationToken cancellationToken)
		{
			var domaintEvent = request.DomainEvent;

			var details = domaintEvent.DetailDtos;
			var history = new HistoryIssue
			{
				IssueId = domaintEvent.IssueId,
				IssueNumber = domaintEvent.IssueNumber,
				ClientId = domaintEvent.ClientId,
				StatusAfter = domaintEvent.IssueStatus,
				PerformedBy = domaintEvent.UserId,
				DateTime = _dateTimeProvider.UtcNow,
				Details = details
				.Select(d => new HistoryIssueDetail
				{
					PalletId = d.PalletId,
					PalletNumber = d.PalletNumber,
					LocationId = d.LocationId,
					LocationSnapShot = d.LocationSnapShot,
				})
				.ToList(),
				Items = domaintEvent.Detailsitems
				.Select(i => new HistoryIssueItems
				{
					ProductId = i.ProductId,
					Quantity = i.Quantity,
					BestBefore = i.BestBedore
				})
				.ToList(),
			};
			_historyIssueRepo.AddHistoryIssue(history);
			return Task.CompletedTask;
		}
	}
}
