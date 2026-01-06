using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Utils
{
	public static class AllocationUtilis
	{
		public static Allocation CreateAllocation(VirtualPallet pallet, Issue issue, int quantity)
		{
			if (pallet == null) throw new NotFoundPalletException($"Brak palety do pickingu o numerze {pallet.PalletId}");
			if (issue == null) throw new NotFoundIssueException(issue.Id);
			if (quantity <= 0) throw new DomainException("Ilość musi być większa od 0");
		var allocation = new Allocation
			{
				Issue = issue,
				VirtualPallet = pallet,
				Quantity = quantity,
				PickingStatus = PickingStatus.Allocated,
			};
			return allocation;
		}
	}
}
