using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Infrastructure.Persistence;
using TestSupport.Seeders.Tables;

namespace TestSupport.Seeders.Scenarios
{
	public static class SeederForClient
	{
		public static void SeedDatabase(WerehouseDbContext context)
		{			
			ClientsSeeder.SeedDatabase(context);
			AddressesSeeder.SeedDatabase(context);
		}
	}
}
