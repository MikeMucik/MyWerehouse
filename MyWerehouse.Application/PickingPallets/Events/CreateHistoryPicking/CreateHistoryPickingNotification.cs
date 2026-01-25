using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Events.CreateHistoryPicking
{
	public record HistoryDataPicking(
		int? PickingTaskId,
		string PalletId,
		int IssueId,
		int ProductId,
		int QuantityAllocated,
		int QuantityPicked,
		PickingStatus StatusBefore,
		PickingStatus StatusAfter,
		string PerformedBy,
		DateTime DateTime);
	
	public record CreateHistoryPickingNotification(
		HistoryDataPicking DataPicking) : INotification;
}
