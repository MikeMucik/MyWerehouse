using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Common.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Test.InMemoryDatabase.Common
{
	public class InMemoryDatabaseFixtureExecutive : IDisposable
	{
		public WerehouseDbContext Context { get; private set; }
		public IMapper Mapper { get; private set; }
		public InMemoryDatabaseFixtureExecutive()
		{
			Context = DbContextFactory.Create().Object;

			var services = new ServiceCollection();
			services.AddLogging();
			services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));
			var serviceProvider = services.BuildServiceProvider();
			Mapper = serviceProvider.GetRequiredService<IMapper>();
		}
		public void Dispose()
		{
			DbContextFactory.Destroy(Context);
		}
	}
	[CollectionDefinition("QueryCollectionInMemory")]//
	public class QueryCollection : ICollectionFixture<InMemoryDatabaseFixtureExecutive> { }

}
