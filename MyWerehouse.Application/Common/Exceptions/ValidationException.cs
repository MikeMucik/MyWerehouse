using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions
{
	public abstract class ValidationException : 		Exception
	{
		protected ValidationException(string message) : base(message) { }
	}
}
