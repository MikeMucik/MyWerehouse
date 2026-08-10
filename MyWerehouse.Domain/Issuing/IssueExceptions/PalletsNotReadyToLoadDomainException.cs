using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class PalletsNotReadyToLoadDomainException : DomainException
	{
		public PalletsNotReadyToLoadDomainException()
			: base("Not all pallets are ready to load.") { }
	}
}
