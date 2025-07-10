using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPalletMovementRepo
	{
		void AddPalletMovement(PalletMovement palletMovement);
		Task AddPalletMovementAsync(PalletMovement palletMovement);
		IQueryable<PalletMovement> GetDataByFilter(PalletMovementSearchFilter filter);
		bool CanDeletePallet(string id);
		Task<bool> CanDeletePalletAsync(string id);		
	}
}
