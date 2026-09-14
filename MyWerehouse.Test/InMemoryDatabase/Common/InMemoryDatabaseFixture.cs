
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.InMemoryDatabase.Common
{
	public class InMemoryDatabaseFixtureExecutive : IDisposable
	{
		public WerehouseDbContext Context { get; private set; }
		public InMemoryDatabaseFixtureExecutive()
		{
			Context = DbContextFactory.Create().Object;

			var services = new ServiceCollection();
			services.AddLogging();
			var serviceProvider = services.BuildServiceProvider();
		}
		public void Dispose()
		{
			DbContextFactory.Destroy(Context);
		}
	}
	[CollectionDefinition("QueryCollectionInMemory")]//
	public class QueryCollection : ICollectionFixture<InMemoryDatabaseFixtureExecutive> { }

}
