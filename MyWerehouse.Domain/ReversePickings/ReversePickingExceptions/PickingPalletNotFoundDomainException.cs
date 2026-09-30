using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.ReversePickings.ReversePickingExceptions
{
	public class PickingPalletNotFoundDomainException : DomainException
	{
		public Guid PickingPalletId { get; }
		public PickingPalletNotFoundDomainException(Guid pickingPalletId)
			: base($"Not found pickingPallet {pickingPalletId}. Cannot reating reverse picking history", Common.ValueObject.ErrorType.InternalError)
		{
			PickingPalletId = pickingPalletId;
		}
	}
}
