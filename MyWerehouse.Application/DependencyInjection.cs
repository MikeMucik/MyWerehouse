using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Common.Behaviors;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using FluentValidation;

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

			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

			services.AddMediatR(cfg =>
			{	cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
				cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			});
			services.AddScoped<IEventCollector, EventCollector>();
			services.AddTransient<ISynchronizerProductsConfig , SynchronizerProductsConfig>();
			return services;
		}
	}
}
