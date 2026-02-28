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
			var results = new List<IssueResult>();
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
			try
			{
				var issue = new Issue(request.DTO.ClientId, request.DTO.PerformedBy, request.Date);
				_issueRepo.AddIssue(issue);
				issue.IssueNumber = await _issueRepo.GetNextNumberOfIssue();

				//foreach (var item in request.DTO.Items)
				//{
				//	var allocation = await _assignProductToIssueService.AssignProductToIssue(issue, item, IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy);
				//}

				issue.IssueItems = new List<IssueItem>();
				foreach (var item in request.DTO.Items)
				{					
					IssueResult addingProducts;
					var result = await _assignProductToIssueService.AssignProductToIssue(issue, item, IssueAllocationPolicy.FullPalletFirst, null, request.DTO.PerformedBy);
					if (result.Success == false)
					{
						 addingProducts = IssueResult.Fail(result.Message, item.ProductId, result.QuantityRequest, result.QuantityOnStock);
					}
					else
					{
						addingProducts = IssueResult.Ok(result.Message, item.ProductId);
					}
					addedProducts.Add(addingProducts);
					var newItem = new IssueItem
					{
						ProductId = item.ProductId,
						IssueNumber = issue.IssueNumber,
						Quantity = item.Quantity,
						BestBefore = item.BestBefore,
					};
					issue.IssueItems.Add(newItem);
				}
				if (addedProducts.Any(r => r.Success == false))
				{
					issue.IssueStatus = IssueStatus.NotComplete;
				}
				issue.AddHistory(request.DTO.PerformedBy);
				await transaction.CommitAsync(ct);
				await _werehouseDbContext.SaveChangesAsync(ct);				
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