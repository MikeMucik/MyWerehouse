using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyWerehouse.Infrastructure.Persistence;
using TestSupport;

namespace MyWerehouse.Api.Tests
{
	public class ApiWebApplicationFactory : WebApplicationFactory<Program>
	{
		private readonly SqliteTestDatabase _database = new();

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseEnvironment("Testing");

			builder.ConfigureTestServices(services =>
			{
				services.RemoveAll<WerehouseDbContext>();
				services.RemoveAll<DbContextOptions<WerehouseDbContext>>();
				services.RemoveAll<IDbContextOptionsConfiguration<WerehouseDbContext>>();

				services.AddDbContext<WerehouseDbContext>(options =>
				{
					_database.ConfigureOptions(options);
				});

			});
		}
		protected override void Dispose(bool disposing)
		{
			try
			{
				base.Dispose(disposing);
			}
			finally
			{
				if (disposing)
					_database.Dispose();
			}
		}

		public void ResetAndSeedDatabase(Action<WerehouseDbContext> seed)
		{
			using var scope = Services.CreateScope();

			var context = scope.ServiceProvider
				.GetRequiredService<WerehouseDbContext>();
			context.Database.EnsureDeleted();
			context.Database.EnsureCreated();
						
			seed(context);
		}
	}
}
