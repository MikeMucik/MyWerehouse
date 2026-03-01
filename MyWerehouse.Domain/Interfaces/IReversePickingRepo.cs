using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IReversePickingRepo
	{
		void AddReversePicking(ReversePicking reversePicking);		
		Task<ReversePicking> GetReversePickingAsync(Guid reversePickingId);
		IQueryable<ReversePicking> GetReversePickings();
		Task<bool> ExistsForPickingPalletAsync(string palletId);
	}
}
