using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common.ValueObject;

namespace MyWerehouse.Domain.Common
{
	public abstract class DomainException : Exception
	{
		public ErrorType ErrorType { get; }

		protected DomainException(string message, ErrorType errorType = ErrorType.Conflict)
			: base(message)
		{
			ErrorType = errorType;
		}
	}
}
