using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Inventories.Services
{
	public interface IGetProductCountService
	{
		Task<int> GetProductCountAsync(int productId, DateOnly? bestBefore);
	}
}
