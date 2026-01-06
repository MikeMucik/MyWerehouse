using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions
{
	public abstract class AppException : Exception
	{
		protected AppException(string message) : base(message) { }
	}
	
	//public abstract class BusinessRuleException : AppException
	//{
	//	protected BusinessRuleException(string message) : base(message) { }
	//}

	public abstract class ValidationException : AppException
	{
		protected ValidationException(string message) : base(message) { }
	}

}
