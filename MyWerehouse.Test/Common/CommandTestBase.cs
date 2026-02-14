using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Infrastructure;


namespace MyWerehouse.Test.Common
{
	public class CommandTestBase : IDisposable
	{
		
		protected readonly WerehouseDbContext _context;
		protected readonly IMapper _mapper;
		public CommandTestBase()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var services = new ServiceCollection();
			services.AddLogging();
			services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
			var serviceProvider = services.BuildServiceProvider();
			_mapper = serviceProvider.GetRequiredService<IMapper>();

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