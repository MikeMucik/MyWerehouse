using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IPickingPalletRepo
	{
		Task AddPalletToPickingAsync(string palletId);
		//Task UpdatePalletPickingAsync(int id, int issueId, int quantity); // dodaje kolejny rekord do palety
		Task DeletePalletPickingAsync(int id);
		Task AddAllocationAsync(int id, int issueId, int quantity);
		Task<List<PickingPallet>> GetPickingPalletsAsync(int productId);
	}
}
