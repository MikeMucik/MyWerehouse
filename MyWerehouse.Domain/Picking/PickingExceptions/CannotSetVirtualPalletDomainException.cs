using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class CannotSetVirtualPalletDomainException : DomainException
	{
		public Guid PickingTaskId { get; }
		public CannotSetVirtualPalletDomainException(Guid id)
			: base($"Task {id} has already virtualPallet.")
		{
			PickingTaskId = id;
		}
	}
}