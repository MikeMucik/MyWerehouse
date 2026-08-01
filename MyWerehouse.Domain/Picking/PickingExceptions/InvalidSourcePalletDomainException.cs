using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class InvalidSourcePalletDomainException :DomainException
	{
		public Guid PickingTaskId { get; }
		public Guid PalletIdFromTask { get; }
		public Guid PalletIdFromUser { get; }
		public InvalidSourcePalletDomainException(Guid pickingTaskId, Guid palletIdFromTask, Guid palletIdFromUser)
			: base ($"Pallet {palletIdFromUser} provided by the user does not belong to picking task {pickingTaskId}, should be pallet {palletIdFromTask}.")
		{
			PickingTaskId = pickingTaskId;
			PalletIdFromTask = palletIdFromTask;
			PalletIdFromUser = palletIdFromUser;
		}
	}
}
