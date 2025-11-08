using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediatR;
//using MediatR.Licensing;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Common.Events;
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
			services.AddTransient<IHistoryService, HistoryService>();
			services.AddTransient<IInventoryService, InventoryService>();
			services.AddTransient<IIssueService, IssueService>();
			services.AddTransient<ILocationService, LocationService>();			
			services.AddTransient<IPalletService, PalletService>();
			services.AddTransient<IPickingPalletService, PickingPalletService>();			
			services.AddTransient<IProductService, ProductService>();
			services.AddTransient<IReceiptService, ReceiptService>();			
			services.AddAutoMapper(Assembly.GetExecutingAssembly());

			//services.AddSingleton<global::MediatR.Licensing.ILicense, global::MediatR.Licensing.OpenSourceLicense>();
			services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));
			
			services.AddScoped<IEventCollector, EventCollector>();

			return services;
		}
	}
}
