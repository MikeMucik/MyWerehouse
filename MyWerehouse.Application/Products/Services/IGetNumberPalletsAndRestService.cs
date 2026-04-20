using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Products.Services
{
	public interface IGetNumberPalletsAndRestService
	{
		//Task<AssignPallestResult> GetNumbers(Guid productId, int AmountUnits);
		Task<int> GetBackOnlyFullPallets(Guid productId, int amountUnits);
	}
}
