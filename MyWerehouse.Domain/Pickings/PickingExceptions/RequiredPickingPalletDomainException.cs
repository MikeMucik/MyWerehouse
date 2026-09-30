using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Pickings.PickingExceptions
{
	public class RequiredPickingPalletDomainException : DomainException
	{
		public RequiredPickingPalletDomainException()
			:base ("Picking pallet id is required.", Common.ValueObject.ErrorType.InternalError) { }
	}
}
