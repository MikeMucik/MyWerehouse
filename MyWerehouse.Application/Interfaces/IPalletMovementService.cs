using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IPalletMovementService
	{
		Task CreateHistoryIssueAsync(Issue issue, IssueStatus status, string userId, IEnumerable<HistoryIssueDetail> details);
		Task CreateMovementAsync(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, 
			PalletStatus palletStatus, IEnumerable<PalletMovementDetail>? details);
		Task CreateMovementAsync(Pallet pallet, string userId, PalletStatus newStatus); //overload
	}
}
