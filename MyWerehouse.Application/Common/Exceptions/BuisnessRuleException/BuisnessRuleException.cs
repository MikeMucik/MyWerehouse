using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.BuisnessRuleException
{
	public abstract class BusinessRuleException : AppException
	{
		protected BusinessRuleException(string message) : base(message) { }
	}
}
