using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Infrastructure.Persistence
{
	public static class DbLength
	{
		public const int Name = 250;
		public const int NameShort = 40;
		public const int Country = 25;

		public const int Street = 95;
		public const int StreetNumber = 20;
		public const int City = 100;
		public const int PostalCode = 20;

		public const int SKU = 50;
		public const int Email = 254;
		public const int DocumentNumber = 50;
		public const int LocationCode = 20;

		public const int Notes = 1000;
	}
}
