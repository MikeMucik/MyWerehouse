using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public abstract class NotFoundException : AppException
	{
		protected NotFoundException(string message) : base(message) { }
	}
}
