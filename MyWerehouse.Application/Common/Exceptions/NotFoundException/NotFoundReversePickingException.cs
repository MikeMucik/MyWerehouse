using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundReversePickingException : NotFoundException
	{
		public int ReversePickingId { get; set; }

		public NotFoundReversePickingException(int reversePickingId)
			: base($"Brak zadania do odróconej kompletacji o numerze {reversePickingId}")
		{
			ReversePickingId = reversePickingId;
		}
		public NotFoundReversePickingException(string message) : base(message) { }
	}
}
