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
using MyWerehouse.Application.Common.DateTimeProvider;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Inventories.Services;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Application.Picking.Services;
using MyWerehouse.Application.Products.Services;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Application.Services;
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
						
			services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

			services.AddMediatR(typeof(ApplicationAssemblyMarker).Assembly);
			services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);			
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));			
			
			services.AddScoped<IAddPickingTaskToIssueService, AddPickingTaskToIssueService>();
			services.AddScoped<IProcessPickingActionService, ProcessPickingActionService>();
			services.AddScoped<IGetProductCountService, GetProductCountService>();
			services.AddScoped<IGetNumberPalletsAndRestService, GetNumberPalletsAndRestService>();
			services.AddScoped<IAssignProductToIssueService, AssignProductToIssueAsyncService>();
			services.AddScoped<IComparePlanToPreparedService, ComparePlanToPreparedService>();
			services.AddScoped<IAddProductsToPalletService, AddProductsToPalletService>();
			services.AddScoped<ICreateReversePickingService, CreateReversePickingService>();
			
			services.AddScoped<IPickingDomainService, PickingDomainService>();			
			
			return services;
		}
	}
}
