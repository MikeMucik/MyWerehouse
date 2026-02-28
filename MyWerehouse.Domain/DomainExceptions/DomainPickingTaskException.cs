using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainPickingTaskException : DomainException
	{
		public string Message { get; set; }
		public DomainPickingTaskException(string message)
			:base(message)
		{
			Message = message;
		}
	}
}
