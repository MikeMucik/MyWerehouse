using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Events.CreateHistoryIssue;
using MyWerehouse.Application.Issues.IssuesServices;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Events;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.CreateNewIssue
{
	public class CreateNewIssueHandler : IRequestHandler<CreateNewIssueCommand, List<IssueResult>>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IIssueRepo _issueRepo;
		private readonly IMediator _mediator;
		private readonly IAssignProductToIssueService _assignProductToIssueService;
		private readonly IEventCollector _eventCollector;
		public CreateNewIssueHandler(WerehouseDbContext werehouseDbContext,
			IIssueRepo issueRepo,
			IMediator mediator,
			IAssignProductToIssueService assignProductToIssueService,
			IEventCollector eventCollector)
		{
			_werehouseDbContext = werehouseDbContext;
			_issueRepo = issueRepo;
			_mediator = mediator;
			_assignProductToIssueService = assignProductToIssueService;
			_eventCollector = eventCollector;
		}
		public async Task<List<IssueResult>> Handle(CreateNewIssueCommand request, CancellationToken ct)
		{
			var addedProducts = new List<IssueResult>();
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
			try
			{
				var issue = new Issue(request.DTO.ClientId, request.DTO.PerformedBy, request.Date);
				_issueRepo.AddIssue(issue);
				issue.IssueItems = new List<IssueItem>();
				//var addedProducts = new List<IssueResult>();
				foreach (var item in request.DTO.Items)
				{					
					IssueResult notAddedProducts;
					var result = await _assignProductToIssueService.AssignProductToIssue(issue, item, IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy);
					if (result.Success == false)
					{
						 notAddedProducts = IssueResult.Fail(result.Message, item.ProductId, result.QuantityRequest, result.QuantityOnStock);
					}
					else
					{
						notAddedProducts = IssueResult.Ok(result.Message, item.ProductId);
					}
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
				await transaction.CommitAsync(ct);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await _mediator.Publish(new AddHistoryForIssueNotification(issue.Id, request.DTO.PerformedBy), ct);//
				
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(factory(), ct);
				}
				return addedProducts;
			}
			catch (DomainException ex)
			{
				await transaction.RollbackAsync(ct);

				addedProducts.Add(IssueResult.Fail(ex.Message));
				return addedProducts;
			}
			finally
			{
				_eventCollector.Clear();
			}			
		}
	}
}