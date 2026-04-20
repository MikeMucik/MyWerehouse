using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public abstract class DomainException : Exception
	{
		//public string Message { get; set; }
		//public string Code { get; }
		protected DomainException(string message) : base(message) { }
		//public DomainException(string message) { Message = message; }
	}
}
