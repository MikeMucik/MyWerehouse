using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.Common
{
	public class QuerryTestFixture : IDisposable
	{
		public WerehouseDbContext Context { get; private set; }
		public IMapper Mapper { get; private set; }
		public QuerryTestFixture()
		{
			Context = DbContextFactory.Create().Object;
			var configurationProvider =
				new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
			Mapper = configurationProvider.CreateMapper();
		}
		public void Dispose()
		{
			DbContextFactory.Destroy(Context);
		}
		[CollectionDefinition("QuerryCollection")]
		public class QuerryCollection : ICollectionFixture<QuerryTestFixture> { }
	}
}
