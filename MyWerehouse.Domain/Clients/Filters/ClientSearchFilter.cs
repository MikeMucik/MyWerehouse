using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Clients.Filters
{
	public class ClientSearchFilter
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		public string FullName { get; set; }		
		public string Country { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public int Phone { get; set; }
		public string PostalCode { get; set; }
		public string StreetName { get; set; }
		public string StreetNumber { get; set; }
	}
}
