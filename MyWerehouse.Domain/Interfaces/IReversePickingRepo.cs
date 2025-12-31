using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IReversePickingRepo
	{
		void AddReversePicking(ReversePicking reversePicking);
		Task AddReversePickingAsync(ReversePicking reversePicking);
		Task<ReversePicking> GetReversePickingAsync(int reversePickingId);
		IQueryable<ReversePicking> GetReversePickings();
	}
}
