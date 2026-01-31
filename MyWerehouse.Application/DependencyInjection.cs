using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Common.Behaviors;
using MyWerehouse.Application.Common.Commands;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.IssuesServices;
using MyWerehouse.Application.Pallets.Services;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Application.Services;

namespace MyWerehouse.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
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
			services.AddTransient<IReversePickingService, ReversePickingService>();
			//services.AddAutoMapper(Assembly.GetExecutingAssembly());
			services.AddAutoMapper(cfg =>cfg.AddMaps(Assembly.GetExecutingAssembly()));
			

			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

			services.AddMediatR(cfg =>
			{
				cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
				cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			});
			services.AddScoped<IEventCollector, EventCollector>();
			services.AddScoped<ICommandCollector, CommandCollector>();
			services.AddTransient<ISynchronizerProductsConfig, SynchronizerProductsConfig>();
			//new services
			services.AddScoped<IAddPickingTaskToIssueService, AddPickingTaskToIssueService>();
			services.AddScoped<ICreatePalletOrAddToPalletService, CreatePalletOrAddToPalletService>();
			services.AddScoped<IProcessPickingActionService, ProcessPickingActionService>();
			services.AddScoped<IReduceAllocationService, ReduceAllocationService>();
			services.AddScoped<IGetVirtualPalletsService, GetVirtualPalletsService>();
			services.AddScoped<IGetProductCountService, GetProductCountService>();
			services.AddScoped<IGetNumberPalletsAndRestService, GetNumberPalletsAndRestService>();
			services.AddScoped<IGetAvailablePalletsByProductService, GetAvailablePalletsByProductService>();
			services.AddScoped<IAssignFullPalletToIssueService, AssignFullPalletToIssueService>();
				
			return services;
		}
	}
}
