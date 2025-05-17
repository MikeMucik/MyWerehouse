using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;

namespace MyWerehouse.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication (this IServiceCollection services)
		{			
			services.AddTransient<ICategoryService, CategoryService>();
			services.AddTransient<IClientService, ClientService>();
			services.AddTransient<IInventoryService, InventoryService>();
			services.AddTransient<IIssueService, IssueService>();
			services.AddTransient<ILocationService, LocationService>();
			services.AddTransient<IPalletMovementService, PalletMovementService>();
			services.AddTransient<IPalletService, PalletService>();
			services.AddTransient<IProductOnPalletService, ProductOnPalletService>();
			services.AddTransient<IProductService, ProductService>();
			services.AddTransient<IReceiptService, ReceiptService>();			
			services.AddAutoMapper(Assembly.GetExecutingAssembly());
			return services;
		}
	}
}
