using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.InMemoryDatabase.Common
{
	public class DbContextFactory
	{
		public static Mock<WerehouseDbContext> Create()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var mock = new Mock<WerehouseDbContext>(options, null) { CallBase = true };
			var context = mock.Object;
			context.Database.EnsureCreated();
			SQLiteInMemoryMode.TestDataSeeder.SeedDatabase(context);
			return mock;
		}

		public static void Destroy(WerehouseDbContext context)
		{
			context.Database.EnsureDeleted();
			context.Dispose();
		}
	}
}
