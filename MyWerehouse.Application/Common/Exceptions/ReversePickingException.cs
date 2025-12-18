using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class ReversePickingException : Exception
	{
		public int ReversePickingId { get; set; }

		public ReversePickingException(int reversePickingId)
			: base($"Brak zadania do odróconej kompletacji o numerze {reversePickingId}")
		{
			ReversePickingId = reversePickingId;
		}
		public ReversePickingException(string message) : base(message) { }
	}
}
