using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Infrastructure.Persistence;


namespace MyWerehouse.Test.InMemoryDatabase.Common
{
	public class CommandTestBase : IDisposable
	{
		
		protected readonly WerehouseDbContext _context;
		public CommandTestBase()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var services = new ServiceCollection();
			services.AddLogging();
			
			var serviceProvider = services.BuildServiceProvider();

			_context = new WerehouseDbContext(options, null);

			_context.Database.EnsureCreated();
		}

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}
