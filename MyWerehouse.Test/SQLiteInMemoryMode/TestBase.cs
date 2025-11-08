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
		//private readonly DbContextOptions<WerehouseDbContext> _contextOptions;

		private readonly ServiceProvider _provider;//

		protected readonly WerehouseDbContext DbContext;
		protected readonly IMediator Mediator;//
		protected TestBase()
		{
			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			//_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
			//	.UseSqlite(_connection)
			//	.Options;

			var services = new ServiceCollection();

			services.AddDbContext<WerehouseDbContext>(opt =>
			opt.UseSqlite(_connection));
			services.AddLogging(config => config.AddConsole());
			services.AddApplication();
			services.AddInfrastructure();
			
			_provider = services.BuildServiceProvider();
			
			DbContext = _provider.GetRequiredService<WerehouseDbContext>();
			//DbContext = new WerehouseDbContext(_contextOptions);			

			//rejestracja MediatR

			Mediator = _provider.GetRequiredService<IMediator>();

			DbContext.Database.EnsureCreated();
		}
		protected WerehouseDbContext CreateNewContext()
		{
		var scope = _provider.CreateScope();
    return scope.ServiceProvider.GetRequiredService<WerehouseDbContext>();}
			//=>
			////new(_contextOptions);
			//_provider.GetRequiredService<WerehouseDbContext>();
		public void Dispose() 
		{
			_provider?.Dispose();
			_connection.Close();
			_connection.Dispose();
			//DbContext.Dispose(); 
			GC.SuppressFinalize(this);
		}
		
	}
}
