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
			services.AddTransient<ICategoryRepo, CategoryRepo>();
			services.AddTransient<IClientRepo, ClientRepo>();
			services.AddTransient<IHistoryIssueRepo, HistoryIssueRepo>();
			services.AddTransient<IInventoryRepo, InventoryRepo>();
			services.AddTransient<IIssueRepo, IssueRepo>();
			services.AddTransient<ILocationRepo, LocationRepo>();
			services.AddTransient<IPalletMovementRepo, PalletMovementRepo>();
			services.AddTransient<IPalletRepo, PalletRepo>();
			services.AddTransient<IPickingPalletRepo, PickingPalletRepo>();
			services.AddTransient<IProductOnPalletRepo, ProductOnPalletRepo>();
			services.AddTransient<IProductRepo, ProductRepo>();
			services.AddTransient<IReceiptRepo, ReceiptRepo>();					
			return services;
		}
	}
}
