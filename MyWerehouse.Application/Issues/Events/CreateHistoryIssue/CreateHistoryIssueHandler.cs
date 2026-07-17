using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Events;

namespace MyWerehouse.Application.Issues.Events.CreateHistoryIssue
{
	public class CreateHistoryIssueHandler(IHistoryIssueRepo historyIssueRepo, IDateTimeProvider dateTimeProvider) 
		: INotificationHandler<AddHistoryForIssueNotification>
	{	
		private readonly IHistoryIssueRepo _historyIssueRepo = historyIssueRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public Task Handle(AddHistoryForIssueNotification request, CancellationToken cancellationToken)
		{		
			var details = request.DetailDtos;
			var history = new HistoryIssue
			{
				IssueId =request.IssueId,
				IssueNumber = request.IssueNumber,
				ClientId = request.ClientId,
				StatusAfter = request.IssueStatus,
				PerformedBy = request.UserId,
				DateTime = _dateTimeProvider.UtcNow,
				Details = details
				.Select(d=> new HistoryIssueDetail
				{
					PalletId = d.PalletId,
					PalletNumber = d.PalletNumber,
					LocationId = d.LocationId,
					LocationSnapShot = d.LocationSnapShot,
				})
				.ToList(),
				Items = request.Detailsitems
				.Select(i=>new HistoryIssueItems
				{
					ProductId = i.ProductId,
					Quantity= i.Quantity,
					BestBefore = i.BestBedore
				})
				.ToList(),				
			};
			_historyIssueRepo.AddHistoryIssue(history);
			return Task.CompletedTask;
		}
	}
}
