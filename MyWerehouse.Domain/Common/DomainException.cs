using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Common
{
	public abstract class DomainException : Exception
	{
		//public string Code { get; }
		protected DomainException(string message) : base(message) { }
	}
}
