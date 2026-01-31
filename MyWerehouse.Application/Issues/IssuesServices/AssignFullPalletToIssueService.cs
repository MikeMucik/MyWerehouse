using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.IssuesServices
{
	public class AssignFullPalletToIssueService : IAssignFullPalletToIssueService
	{
		private readonly IEventCollector _eventCollector;
		public AssignFullPalletToIssueService(IEventCollector eventCollector)
		{
			_eventCollector = eventCollector;
		}

		public Task AddPallets(Issue issue, List<Pallet> pallets)
		{
			foreach (var pallet in pallets)// adding full pallets *
			{
				pallet.IssueId = issue.Id;
				// No change Status -> verify
				_eventCollector.Add(new CreatePalletOperationNotification(pallet.Id,
				pallet.LocationId,
				ReasonMovement.ToLoad,
				issue.PerformedBy,
				PalletStatus.InTransit,
				null));
				issue.Pallets.Add(pallet);
			}
			return Task.CompletedTask;
		}
	}
}
