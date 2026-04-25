using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Picking.PickingExceptions
{
	public class RequiredPickingPalletException : DomainException
	{
		public RequiredPickingPalletException()
			:base ("Picking pallet id is required.") { }
	}
}
