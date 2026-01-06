using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue
{
	public class AssignFullPalletToIssueHandler :IRequestHandler<AssignFullPalletToIssueCommand, List<Pallet>>
	{			
		private readonly IEventCollector _eventCollector;
		public AssignFullPalletToIssueHandler(		
			IEventCollector eventCollector)
		{			
			_eventCollector = eventCollector;
		}
		public  Task<List<Pallet>> Handle(AssignFullPalletToIssueCommand request, CancellationToken ct)
		{
			var palletsToAsign = request.Pallets
					.OrderByDescending(p => p.ProductsOnPallet.First(po => po.Quantity > 0).Quantity)
					.Take(request.FullPalletCount)
					.ToList();
			foreach (var pallet in palletsToAsign)// adding full pallets *
			{
				pallet.IssueId = request.Issue.Id;

				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id,
				pallet.LocationId,
				ReasonMovement.ToLoad,
				request.Issue.PerformedBy,
				PalletStatus.InTransit,
				null));

				request.Issue.Pallets.Add(pallet);
			}
			return Task.FromResult(palletsToAsign);
		}
	}
}
