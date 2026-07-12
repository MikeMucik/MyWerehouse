using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Common
{
	public abstract class DomainException : Exception
	{		
		protected DomainException(string message) : base(message) { }
	}
}