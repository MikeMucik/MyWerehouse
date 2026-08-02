using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public interface IAddProductsToPalletService
	{
		Task<ReversePickingResult> AddProductsToSourcePallet(ReversePickingTask task, string userId);
		Task<ReversePickingResult> AddToExistingPallet(ReversePickingTask task, List<Guid> pallets, string userId);
		Task<ReversePickingResult> AddToNewPallet(ReversePickingTask task, string userId, int rampNumber, string snapShot);
	}
}
