using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence.Repositories;

namespace MyWerehouse.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services)
		{			
			services.AddScoped<IPickingTaskRepo, PickingTaskRepo>();
			services.AddScoped<ICategoryRepo, CategoryRepo>();
			services.AddScoped<IClientRepo, ClientRepo>();

			services.AddScoped<IHistoryIssueRepo, HistoryIssueRepo>();
			services.AddScoped<IHistoryReceiptRepo, HistoryReceiptRepo>();
			services.AddScoped<IHistoryPickingRepo, HistoryPickingRepo>();
			services.AddScoped<IHistoryReversePickingRepo, HistoryReversePickingRepo>();

			services.AddScoped<IInventoryRepo, InventoryRepo>();
			services.AddScoped<IIssueRepo, IssueRepo>();
			
			services.AddScoped<ILocationRepo, LocationRepo>();
			services.AddScoped<IHistoryPalletRepo, HistoryPalletRepo>();
			services.AddScoped<IPalletRepo, PalletRepo>();
			services.AddScoped<IVirtualPalletRepo, VirtualPalletRepo>();
			services.AddScoped<IProductRepo, ProductRepo>();
			services.AddScoped<IReceiptRepo, ReceiptRepo>();
			services.AddScoped<IReversePickingRepo, ReversePickingRepo>();

			return services;
		}
	}
}
