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
		//void RegisterPalletMovement(string palletId, int productId, int location, int quantity, ReasonMovement reason, string performedBy);
		//void CreateMovement(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, IEnumerable<PalletMovementDetail> details);
		Task CreateHistoryIssueAsync(Issue issue, IssueStatus status, string userId, IEnumerable<HistoryIssueDetail> details);
		Task CreateMovementAsync(Pallet pallet, int destinationLocationId, ReasonMovement reasonMovement, string userId, IEnumerable<PalletMovementDetail> details);

	}
}
