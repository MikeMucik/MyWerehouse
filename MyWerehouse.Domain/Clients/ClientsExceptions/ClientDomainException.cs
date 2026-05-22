using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Clients.ClientsExceptions
{
	public class ClientDomainException : DomainException
	{
		public ClientDomainException():base("Client number must greater than zero. ") { }
	}
}
