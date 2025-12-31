using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class InvalidUserIdException : DomainException
	{
		public InvalidUserIdException(string userId)
			: base($"Nieprawidłowy lub brak numeru użytkownika {userId}") { }
	}
}
