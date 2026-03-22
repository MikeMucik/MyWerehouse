using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Services
{
	public interface ICreatePalletOrAddToPalletService
	{
		Task<CreatePalletResult> CreatePalletOrAddToPallet(Issue issue, Guid productId, int quantity, string userId, DateOnly? bestBefore, PickingTask pickingTask, PickingCompletion pickingCompletion);
	}
}
