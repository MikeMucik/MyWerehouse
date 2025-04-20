using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Test.Common
{
	public class DbContextFactory
	{
		public static Mock<WerehouseDbContext> Create()
		{
			var options = new DbContextOptionsBuilder<WerehouseDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;
			var mock = new Mock<WerehouseDbContext>(options) { CallBase = true};
			var context = mock.Object;
			context.Database.EnsureCreated();
			SeedDatabase(context);
			return mock;
		}
		public static void SeedDatabase(WerehouseDbContext context)
		{//tu dane do bazy InMemory
			if (!context.Categories.Any())
			{
				var category = new Category
				{
					Id = 1,
					Name = "TestCategory",
				};
				context.Add(category);
				var category1 = new Category
				{
					Id = 2,
					Name = "TestCategory1",
				};
				context.Add(category1);
			}
			if (!context.Products.Any())
			{
				var product = new Product
				{
					Id = 10,
					Name = "Test",
					SKU = "0987654321",
					CategoryId = 1,
				};
				context.Add(product);
				var productToD = new Product
				{
					Id = 11,
					Name = "TestD",
					SKU = "fghtredfg",
					CategoryId = 1,
				};
				context.Add(productToD);
			}
			var productD = new ProductDetails
			{
				Id = 10,
				ProductId = 10,
				Length = 10,
				Height = 20,
				Width = 30,
				Weight = 2,
				Description = "Test",

			};
			context.ProductDetails.Add(productD);
			context.SaveChanges();
		}
		public static void Destroy(WerehouseDbContext context)
		{
			context.Database.EnsureDeleted();
			context.Dispose();
		}
	}
}
