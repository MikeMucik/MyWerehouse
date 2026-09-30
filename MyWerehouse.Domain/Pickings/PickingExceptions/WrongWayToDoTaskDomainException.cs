using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Pickings.PickingExceptions
{
	public class WrongWayToDoTaskDomainException : DomainException
	{
		public WrongWayToDoTaskDomainException()
				: base("Source pallet is not prepared to picking.")
		{}
	}
}
