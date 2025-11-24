using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediatR;
//using MediatR.Licensing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyWerehouse.Application;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode
{
	public abstract class TestBase : IDisposable
	{
		private readonly SqliteConnection _connection;
		public readonly ServiceProvider _provider;
		public readonly WerehouseDbContext DbContext;
		public readonly IMediator Mediator;
		public TestBase()
		{
			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			var services = new ServiceCollection();

			services.AddDbContext<WerehouseDbContext>(opt =>
			opt.UseSqlite(_connection));
			services.AddLogging(config => config.AddConsole());
			services.AddApplication();
			services.AddInfrastructure();
			_provider = services.BuildServiceProvider();
			DbContext = _provider.GetRequiredService<WerehouseDbContext>();
			Mediator = _provider.GetRequiredService<IMediator>();
			DbContext.Database.EnsureCreated();
		}
		protected WerehouseDbContext CreateNewContext()
		{
			var scope = _provider.CreateScope();
			return scope.ServiceProvider.GetRequiredService<WerehouseDbContext>();
		}

		public void Dispose()
		{
			_provider?.Dispose();
			_connection.Close();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
