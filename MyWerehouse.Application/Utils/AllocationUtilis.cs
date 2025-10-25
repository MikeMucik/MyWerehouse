using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Exceptions;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Utils
{
	public static class AllocationUtilis
	{
		public static Allocation CreateAllocation(VirtualPallet pallet, Issue issue, int quantity)
		{
			if (pallet == null) throw new PalletNotFoundException($"Brak palety do pickingu o numerze {pallet.PalletId}");
			if (issue == null) throw new IssueNotFoundException(issue.Id);
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
