using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Common
{
	public class WrongDateDomainException :DomainException
	{
		public WrongDateDomainException()
		:base("Wrong data, check date."){ }
	}
}
