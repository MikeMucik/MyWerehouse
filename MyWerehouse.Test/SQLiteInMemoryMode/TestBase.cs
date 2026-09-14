using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MyWerehouse.Application;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Common;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Infrastructure.Persistence.ReadServices;

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
			//services.AddScoped<IUnitOfWork, UnitOfWork>();
			////refactor clean architecture
			//services.AddScoped<IPalletReadService, PalletReadService>();
			//services.AddScoped<IIssueReadService, IssueReadService>();
			//services.AddScoped<IReceiptReadService, ReceiptReadService>();
			//services.AddScoped<IPickingReadService, PickingReadService>();
			//services.AddScoped<IReversePickingReadService, ReversePickingReadService>();
			//services.AddScoped<IInventoryReadService, InventoryReadService>();
			//services.AddScoped<IHistoryReadService,  HistoryReadService>();

			services.AddInfrastructure();
			services.RemoveAll<IDateTimeProvider>();
			services.AddSingleton<IDateTimeProvider, TestDateTimeProvider>();//stały czas dla testów
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
