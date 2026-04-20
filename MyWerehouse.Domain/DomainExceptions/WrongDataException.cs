using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class WrongDataException :DomainException
	{
		public WrongDataException()
		:base("Wrong data, date can't be from past."){ }
	}
}
