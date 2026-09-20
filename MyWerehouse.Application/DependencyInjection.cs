using MyWerehouse.Application.Pallets.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Categories.Services;
using MyWerehouse.Application.Clients.Services;
using MyWerehouse.Application.Common.Behaviors;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Application.Locations.Services;
using MyWerehouse.Application.Pickings.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Services;

namespace MyWerehouse.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			services.AddScoped<ICategoryService, CategoryService>();
			services.AddScoped<IClientService, ClientService>();
			services.AddScoped<ILocationService, LocationService>();
			services.AddScoped<IProductService, ProductService>();
			services.AddScoped<IPalletNumberAllocator, PalletNumberAllocator>();		

			services.AddMediatR(typeof(ApplicationAssemblyMarker).Assembly);
			services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

			services.AddScoped<IAddPickingTaskToIssueService, AddPickingTaskToIssueService>();
			services.AddScoped<IExecuteProcessPickingService, ExecuteProcessPickingService>();

			services.AddScoped<IAssignProductToIssueService, AssignProductToIssueAsyncService>();
			services.AddScoped<IAddProductsToPalletService, AddProductsToPalletService>();
			services.AddScoped<ICreateReversePickingService, CreateReversePickingService>();

			services.AddScoped<IPickingDomainService, PickingDomainService>();

			return services;
		}
	}
}
