using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Configuration;
using MyWerehouse.Infrastructure.Persistence.Repositories;

namespace MyWerehouse.Infrastructure.Persistence
{
	public class WerehouseDbContext : IdentityDbContext
	{
		private readonly IPublisher _publisher;
		public WerehouseDbContext(DbContextOptions<WerehouseDbContext> options, IPublisher? publisher) : base(options)
		{
			_publisher = publisher;
		}
		public DbSet<Address> Addresses { get; set; }
		public DbSet<PickingTask> PickingTasks { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<HistoryIssue> HistoryIssues { get; set; }
		public DbSet<HistoryIssueDetail> HistoryIssueDetails { get; set; }
		public DbSet<HistoryReceipt> HistoryReceipts { get; set; }//																
		public DbSet<HistoryReceiptDetail> HistoryReceiptDetails { get; set; }//																
		public DbSet<HistoryPicking> HistoryPickings { get; set; }//		
		public DbSet<HistoryReversePicking> HistoryReversePickings { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<Issue> Issues { get; set; }
		public DbSet<IssueItem> IssueItems { get; set; }
		public DbSet<Location> Locations { get; set; }
		public DbSet<Pallet> Pallets { get; set; }
		public DbSet<PalletMovement> PalletMovements { get; set; }
		public DbSet<PalletMovementDetail> PalletMovementDetails { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<ProductDetail> ProductDetails { get; set; }
		public DbSet<ProductOnPallet> ProductOnPallet { get; set; }
		public DbSet<Receipt> Receipts { get; set; }
		public DbSet<ReversePicking> ReversePickings { get; set; }
		public DbSet<VirtualPallet> VirtualPallets { get; set; }

		public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			await DispatherDomainEventAsync();
			return await base.SaveChangesAsync(ct);
		}
		private async Task DispatherDomainEventAsync()
		{
			var domainEntities = ChangeTracker
				.Entries<AggregateRoots>()
				.Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any())
				.Select(x=>x.Entity)
				.ToList();

			if(!domainEntities.Any())
			{
				return;
			}

			if (_publisher == null) return;

			var domainEvents = domainEntities
				.SelectMany(x => x.DomainEvents)
				.ToList();

			domainEntities.ForEach(entity => entity.ClearDomainEvents());

			foreach (var domainEvent in domainEvents)
			{
				await _publisher.Publish(domainEvent);
			}
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var provider = Database.ProviderName;

			modelBuilder.ApplyConfiguration(new AddressConfiguration(provider));			
			modelBuilder.ApplyConfiguration(new CategoryConfiguration(provider));
			modelBuilder.ApplyConfiguration(new ClientConfiguration(provider));
			modelBuilder.ApplyConfiguration(new HistoryIssueConfiguration());
			modelBuilder.ApplyConfiguration(new HistoryIssueDetailConfiguration());
			modelBuilder.ApplyConfiguration(new HistoryReceiptConfiguration());
			modelBuilder.ApplyConfiguration(new HistoryReceiptDetailConfiguration());
			modelBuilder.ApplyConfiguration(new HistoryPickingConfiguration());
			modelBuilder.ApplyConfiguration(new HistoryReversePickingConfiguration());
			modelBuilder.ApplyConfiguration(new InventoryConfiguration());			
			modelBuilder.ApplyConfiguration(new IssueConfiguration());
			modelBuilder.ApplyConfiguration(new IssueItemConfiguration());
			modelBuilder.ApplyConfiguration(new LocationConfiguration());
			modelBuilder.ApplyConfiguration(new PalletConfiguration());
			modelBuilder.ApplyConfiguration(new PalletMovementConfiguration());
			modelBuilder.ApplyConfiguration(new PalletMovementDetailConfiguration());
			modelBuilder.ApplyConfiguration(new PickingTaskConfiguration());
			modelBuilder.ApplyConfiguration(new ProductConfiguration(provider));
			modelBuilder.ApplyConfiguration(new ProductDetailConfiguration());
			modelBuilder.ApplyConfiguration(new ProductOnPalletConfiguration());
			modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
			modelBuilder.ApplyConfiguration(new ReversePickingConfiguration());
			modelBuilder.ApplyConfiguration(new VirtualPalletConfiguration());

			base.OnModelCreating(modelBuilder);
		}
	}
}
