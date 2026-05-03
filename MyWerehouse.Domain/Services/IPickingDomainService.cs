using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Domain.Services
{
	public interface IPickingDomainService
	{
		Task<PickingTask> GetSingleHandPickingTask(Guid issueId, Guid productId);
	}
}
