using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

using System.IO;

namespace MyWerehouse.Infrastructure
{
	public class WerehouseDbContextFactory : IDesignTimeDbContextFactory<WerehouseDbContext>
	{
		public WerehouseDbContext CreateDbContext(string[] args)
		{
			// Ścieżka do katalogu głównego projektu startowego (MyWerehouse.Server), gdzie jest appsettings.json
			var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "MyWerehouse.Server");

			// Budujemy konfigurację, żeby pobrać connection string
			var configuration = new ConfigurationBuilder()
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json")
				.Build();

			var optionsBuilder = new DbContextOptionsBuilder<WerehouseDbContext>();
			optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

			return new WerehouseDbContext(optionsBuilder.Options);
		}
	}
}
