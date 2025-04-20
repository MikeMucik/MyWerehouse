using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.Common
{
	public class CommandTestBase : IDisposable
	{
		protected readonly WerehouseDbContext _context;
		protected readonly Mock<WerehouseDbContext> _contextMock;

		public CommandTestBase()
		{
			_contextMock = DbContextFactory.Create();
			_context = _contextMock.Object;
		}
		public void Dispose()
		{
			DbContextFactory.Destroy(_context);
		}
	}
}
