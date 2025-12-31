using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class InvalidRampException : DomainException
	{
		public InvalidRampException(int rampNumber)
			: base($"Nieprawidłowe numer rampy : {rampNumber}.") { }
	}
}
