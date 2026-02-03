using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public interface IAddProductsToPalletService
	{
		ReversePickingResult AddProductsToSourcePallet(ReversePicking task, string userId);
		Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, List<Pallet> pallets, string userId);
		Task<ReversePickingResult> AddToNewPallet(ReversePicking task, string userId);
	}
}
