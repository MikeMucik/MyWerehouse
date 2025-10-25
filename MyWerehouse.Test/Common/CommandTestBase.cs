using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using MyWerehouse.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Data.Sqlite;


namespace MyWerehouse.Test.Common
{
	public class CommandTestBase : IDisposable
	{
		
		protected readonly WerehouseDbContext _context;

		public CommandTestBase()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

			_context = new WerehouseDbContext(options);

			_context.Database.EnsureCreated();
		}

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}
	}
}
//protected readonly Mock<WerehouseDbContext> _contextMock;	
		//public CommandTestBase()
		//{
		//	_contextMock = DbContextFactory.Create();
		//	_context = _contextMock.Object;
		//}

//public void Dispose()
		//{
		//	DbContextFactory.Destroy(_context);
		//}