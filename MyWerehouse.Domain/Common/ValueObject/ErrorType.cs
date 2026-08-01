using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Common.ValueObject
{
	public enum ErrorType
	{
		Validation,
		NotFound,
		Conflict,
	}
}
