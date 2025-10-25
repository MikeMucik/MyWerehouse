using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode
{
	public abstract class TestBase : IDisposable
	{
		private readonly SqliteConnection _connection;
		private readonly DbContextOptions<WerehouseDbContext> _contextOptions;
		protected readonly WerehouseDbContext DbContext;

		protected TestBase()
		{
			_connection = new SqliteConnection("DataSource=:memory:");
			_connection.Open();

			_contextOptions = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseSqlite(_connection)
				.Options;

			DbContext = new WerehouseDbContext(_contextOptions);
			DbContext.Database.EnsureCreated();
		}
		protected WerehouseDbContext CreateNewContext() => new(_contextOptions);
		public void Dispose() 
		{
			_connection.Close();
			_connection.Dispose();
			DbContext.Dispose(); 
			GC.SuppressFinalize(this);
		}
	}
}
