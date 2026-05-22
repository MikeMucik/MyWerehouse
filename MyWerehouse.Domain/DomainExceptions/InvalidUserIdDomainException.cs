using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class InvalidUserIdDomainException : DomainException
	{
		public InvalidUserIdDomainException(string userId)
			: base($"Invalid {userId} or missing user ID.") { }
	}
}