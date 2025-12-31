using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class InvalidClientException : DomainException
	{
		public InvalidClientException(int clientId)
			: base($"Nieprawidłowy numer client {clientId}.") { }
	}
}
