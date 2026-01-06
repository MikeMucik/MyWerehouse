using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.BuisnessRuleException
{
	public class RampNotFoundException : BusinessRuleException
	{
		public RampNotFoundException(int rampNumber)
			: base($"Nieprawidłowe numer rampy : {rampNumber}.") { }
	}
}
