using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class WrongDateDomainException :DomainException
	{
		public WrongDateDomainException()
		:base("Wrong data, date can't be from past."){ }
	}
}
