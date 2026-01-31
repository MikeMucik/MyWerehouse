using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;


namespace MyWerehouse.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services)
		{
			//services.AddTransient<IPickingTaskRepo, PickingTaskRepo>();
			//services.AddTransient<ICategoryRepo, CategoryRepo>();
			//services.AddTransient<IClientRepo, ClientRepo>();

			//services.AddTransient<IHistoryIssueRepo, HistoryIssueRepo>();
			//services.AddTransient<IHistoryReceiptRepo, HistoryReceiptRepo>();
			//services.AddTransient<IHistoryPickingRepo, HistoryPickingRepo>();
			//services.AddTransient<IHistoryReversePickingRepo, HistoryReversePickingRepo>();

			//services.AddTransient<IInventoryRepo, InventoryRepo>();
			//services.AddTransient<IIssueRepo, IssueRepo>();
			//services.AddTransient<IIssueItemRepo, IssueItemRepo>();
			//services.AddTransient<ILocationRepo, LocationRepo>();
			//services.AddTransient<IPalletMovementRepo, PalletMovementRepo>();
			//services.AddTransient<IPalletRepo, PalletRepo>();
			//services.AddTransient<IPickingPalletRepo, PickingPalletRepo>();			
			//services.AddTransient<IProductRepo, ProductRepo>();
			//services.AddTransient<IReceiptRepo, ReceiptRepo>();		
			//services.AddTransient<IReversePickingRepo, ReversePickingRepo>();

			//services.AddTransient<IHandPickingTaskRepo, HandPickingRepo>();
			services.AddScoped<IPickingTaskRepo, PickingTaskRepo>();
			services.AddScoped<ICategoryRepo, CategoryRepo>();
			services.AddScoped<IClientRepo, ClientRepo>();

			services.AddScoped<IHistoryIssueRepo, HistoryIssueRepo>();
			services.AddScoped<IHistoryReceiptRepo, HistoryReceiptRepo>();
			services.AddScoped<IHistoryPickingRepo, HistoryPickingRepo>();
			services.AddScoped<IHistoryReversePickingRepo, HistoryReversePickingRepo>();

			services.AddScoped<IInventoryRepo, InventoryRepo>();
			services.AddScoped<IIssueRepo, IssueRepo>();
			services.AddScoped<IIssueItemRepo, IssueItemRepo>();
			services.AddScoped<ILocationRepo, LocationRepo>();
			services.AddScoped<IPalletMovementRepo, PalletMovementRepo>();
			services.AddScoped<IPalletRepo, PalletRepo>();
			services.AddScoped<IPickingPalletRepo, PickingPalletRepo>();
			services.AddScoped<IProductRepo, ProductRepo>();
			services.AddScoped<IReceiptRepo, ReceiptRepo>();
			services.AddScoped<IReversePickingRepo, ReversePickingRepo>();

			services.AddScoped<IHandPickingTaskRepo, HandPickingRepo>();


			return services;
		}
	}
}
