using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Issues.Commands.AddPalletsToIssueByProduct;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Issues.Commands.CreateNewIssue
{
	public class CreateNewIssueHandler : IRequestHandler<CreateNewIssueCommand, List<IssueResult>>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		public CreateNewIssueHandler(WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
		}
		public async Task<List<IssueResult>> Handle(CreateNewIssueCommand request, CancellationToken cancellationToken)
		{			
			var issue = new Issue(request.DTO.ClientId, request.DTO.PerformedBy, request.Date);
			_issueRepo.AddIssue(issue);
			issue.IssueItems = new List<IssueItem>();
			var addedProducts = new List<IssueResult>();
			foreach (var item in request.DTO.Items)
			{
				var notAddedProducts = await _mediator.Send(new AddPalletsToIssueByProductCommand(issue, item),cancellationToken);
				addedProducts.Add(notAddedProducts);
				var newItem = new IssueItem
				{
					ProductId = item.ProductId,
					Quantity = item.Quantity,
					BestBefore = item.BestBefore,
				};
				issue.IssueItems.Add(newItem);
			}
			if (addedProducts.Any(r => r.Success == false))
			{
				issue.IssueStatus = IssueStatus.NotComplete;
			}
			await _werehouseDbContext.SaveChangesAsync(cancellationToken);
			await _mediator.Publish(new CreateHistoryIssueNotification(issue.Id, request.DTO.PerformedBy), cancellationToken);//
			return addedProducts;
		}
	}
}
