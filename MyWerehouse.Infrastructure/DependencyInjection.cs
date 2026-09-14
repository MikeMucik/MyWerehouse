using Microsoft.Extensions.DependencyInjection;
using MyWerehouse.Application.Common.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Infrastructure.Common;
using MyWerehouse.Infrastructure.Common.DateTimeProvider;
using MyWerehouse.Infrastructure.Common.Events;
using MyWerehouse.Infrastructure.Persistence.ReadServices;
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

			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<ICategoryReadService, CategoryReadService>();
			services.AddScoped<IClientReadService, ClientReadService>();
			services.AddScoped<ILocationReadService, LocationReadService>();
			services.AddScoped<IProductReadService, ProductReadService>();
			services.AddScoped<IPalletReadService, PalletReadService>();
			services.AddScoped<IIssueReadService, IssueReadService>();
			services.AddScoped<IReceiptReadService, ReceiptReadService>();
			services.AddScoped<IPickingReadService, PickingReadService>();
			services.AddScoped<IReversePickingReadService, ReversePickingReadService>();
			services.AddScoped<IInventoryReadService, InventoryReadService>();
			services.AddScoped<IHistoryReadService, HistoryReadService>();

			services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

			services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

			//services.
			return services;
		}
	}
}
