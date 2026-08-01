using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class TooHighValueDomainException :DomainException
	{
		public int RequestedValue { get; }
		public int PickedValue { get; }
		public TooHighValueDomainException(int requestedValue, int pickedValue)
			:base($"Cannot pick {pickedValue} more than requested quantity {requestedValue}.")
		{
			RequestedValue = requestedValue;
			PickedValue = pickedValue;
		}
	}
}
