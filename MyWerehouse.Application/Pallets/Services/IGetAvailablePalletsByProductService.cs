using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.Services
{
	public interface IGetAvailablePalletsByProductService
	{
		Task<List<Pallet>> GetPallets(Guid productId, DateOnly? bestBefore, int Reserved);
	}
}
