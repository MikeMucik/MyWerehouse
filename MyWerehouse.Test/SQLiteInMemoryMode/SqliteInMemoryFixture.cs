using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.SQLiteInMemoryMode
{
	[CollectionDefinition("QueryCollection")]
	public class SqliteInMemoryFixture : ICollectionFixture<QueryTestSQLFixture> { }

	public class QueryTestSQLFixture : TestBase  // Dziedziczy po TestBase (SQLite in-memory)
	{
		public QueryTestSQLFixture()
			: base()  // Wywołuje ctor TestBase (connection, options, EnsureCreated)
		{
			TestDataSeeder.SeedDatabase(DbContext);
		}
		public WerehouseDbContext DbContext => base.DbContext;
		// Opcjonalnie: Metoda do tworzenia czystego kontekstu (bez seedu, dla izolowanych testów)
		public WerehouseDbContext CreateCleanContext() => CreateNewContext();  // Z TestBase
	
	}

}
