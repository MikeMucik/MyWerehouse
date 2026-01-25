using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Infrastructure
{
	public class WerehouseDbContext : IdentityDbContext
	{
		public WerehouseDbContext(DbContextOptions<WerehouseDbContext> options) : base(options) { }
		public DbSet<Address> Addresses { get; set; }
		public DbSet<PickingTask> PickingTasks { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<HandPickingTask> HandPickingTasks { get; set; }
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
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Address>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

				if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
				{
					entity.Property(e => e.Country)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.City)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.Region)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.StreetName)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.PostalCode)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.StreetNumber)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				}
			});
			modelBuilder.Entity<PickingTask>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(a => a.PickingStatus)
				.HasConversion<string>();

				entity.HasOne(a => a.VirtualPallet)
					.WithMany(p => p.PickingTasks)
					.HasForeignKey(a => a.VirtualPalletId);

				entity.HasOne(a => a.Issue)
					 .WithMany(a=>a.PickingTasks)
					 .HasForeignKey(a => a.IssueId);

				entity.HasOne(a => a.PickingPallet)
				.WithMany()
				.HasForeignKey(a => a.PickingPalletId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.Restrict);
			});
			modelBuilder.Entity<Category>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
				if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
				{
					entity.Property(e => e.Name)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				}
			});
			modelBuilder.Entity<Client>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
				if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
				{
					entity.Property(e => e.Name)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.FullName)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.Email)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.Description)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				}
			});
			modelBuilder.Entity<HandPickingTask>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(a => a.PickingStatus)
				.HasConversion<string>();
			});
			modelBuilder.Entity<HistoryIssue>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

				entity.Property(r => r.StatusAfter)
				.HasConversion<string>();
			});
			modelBuilder.Entity<HistoryIssueDetail>(entity =>
			{
				entity.HasOne(h => h.HistoryIssue)
				.WithMany(hd => hd.Details)
				.HasForeignKey(h => h.HistoryIssueId);
			});
			modelBuilder.Entity<HistoryReceipt>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

				entity.Property(r => r.StatusAfter)
				.HasConversion<string>();
			});
			modelBuilder.Entity<HistoryReceiptDetail>(entity =>
			{
				entity.HasOne(h => h.HistoryReceipt)
				.WithMany(hd => hd.Details)
				.HasForeignKey(h => h.HistoryReceiptId);
			});
			modelBuilder.Entity<HistoryPicking>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

				entity.Property(a => a.StatusBefore)
				.HasConversion<string>();

				entity.Property(a => a.StatusAfter)
				.HasConversion<string>();
				//// Indeks dla szybkiego wyszukiwania
				entity.HasIndex(x => x.PalletId);
				entity.HasIndex(x => x.PickingTaskId);
				entity.HasIndex(x => x.IssueId);
				entity.HasIndex(h => h.DateTime);
			});
			modelBuilder.Entity<HistoryReversePicking>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

				entity.Property(a => a.StatusBefore)
				.HasConversion<string>();

				entity.Property(a => a.StatusAfter)
				.HasConversion<string>();
			});
			modelBuilder.Entity<Inventory>(entity =>
			{
				entity.HasKey(e => e.ProductId);
				entity.Property(x => x.ProductId).ValueGeneratedNever();

				entity.HasOne(i => i.Product)
				.WithOne(p => p.InventoryItem)
				.HasForeignKey<Inventory>(i => i.ProductId)
				.OnDelete(DeleteBehavior.Restrict);//do przemyślenia
			});
			modelBuilder.Entity<Issue>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
			
				//entity.HasOne(a => a.Issue)
				//	.WithMany(i => i.PickingTasks)
				//	.HasForeignKey(a => a.IssueId)
				//	.OnDelete(DeleteBehavior.Restrict);

			});
			modelBuilder.Entity<IssueItem>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x=>x.Id).ValueGeneratedOnAdd();		
				
				entity.HasOne(e=>e.Issue)
					.WithMany(a => a.IssueItems)
					.HasForeignKey(a=>a.IssueId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(e => e.Product)
					.WithMany()
					.HasForeignKey(e => e.ProductId)
					.OnDelete(DeleteBehavior.Restrict);

				//entity.HasOne(a => a.Issue)
				//	.WithMany(i => i.HistoryPickings)
				//	.HasForeignKey(h => h.IssueId)
				//	.OnDelete(DeleteBehavior.Restrict);
			});
			modelBuilder.Entity<Location>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
			});
			modelBuilder.Entity<Pallet>(entity =>
			{
				entity.HasKey(p => p.Id);
				entity.Property(p => p.Id)
					.IsRequired()
					.HasMaxLength(10);

				entity.Property(p => p.Status)
				.HasConversion<string>();

				entity.HasMany(p => p.ProductsOnPallet)
					.WithOne(a=>a.Pallet)
					.HasForeignKey(pop => pop.PalletId)
					.IsRequired()
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasMany(p => p.PalletMovements)
					.WithOne()
					.HasForeignKey(h => h.PalletId)
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(p => p.Issue)
					.WithMany(i => i.Pallets)
					.HasForeignKey(p => p.IssueId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.Property(e => e.RowVersion)
				  .IsRowVersion()  // To kluczowe! Oznacza pole jako Timestamp/RowVersion
				  .HasColumnType("rowversion")  // Dla SQL Server; dla innych DB dostosuj
				  .IsRequired(false);  // Opcjonalne, ale Timestamp jest zwykle nullable
			});
			modelBuilder.Entity<PalletMovement>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
				
				entity.Property(m => m.Reason)
				.HasConversion<string>();
			});
			modelBuilder.Entity<PalletMovementDetail>(entity =>
			{
				entity.HasOne(md => md.PalletMovement)
				.WithMany(pm => pm.PalletMovementDetails)
				.HasForeignKey(md => md.PalletMovementId);
			});
			modelBuilder.Entity<VirtualPallet>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(entity => entity.Id).ValueGeneratedOnAdd();

				entity.HasMany(p => p.PickingTasks)
						.WithOne(a => a.VirtualPallet)//tu był błąd jendokierunkowy
						.HasForeignKey(p => p.VirtualPalletId)
						.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(vp => vp.Location)
						.WithMany()
						.HasForeignKey(vp => vp.LocationId)
						.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(vp => vp.Pallet)
						.WithMany()
						.HasForeignKey(vp => vp.PalletId)
						.OnDelete(DeleteBehavior.Restrict);

			});
			modelBuilder.Entity<Product>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();
				if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
				{
					entity.Property(e => e.Name)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
					entity.Property(e => e.SKU)
					.HasMaxLength(20)
					.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				}

				entity.HasOne(pm => pm.InventoryItem)
				.WithOne(i => i.Product)
				.HasForeignKey<Inventory>(i => i.ProductId);

				entity.HasOne(p => p.Details)
				.WithOne(p => p.Product)
				.HasForeignKey<ProductDetail>(p => p.ProductId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<ProductDetail>(entity =>
			{
				entity.HasKey(e => e.ProductId);
				entity.Property(e => e.ProductId).ValueGeneratedNever();
			});
			modelBuilder.Entity<ProductOnPallet>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

				entity.Property(e => e.PalletId)
				.IsRequired()
				.HasMaxLength(10);

				entity.HasOne(pop => pop.Pallet)
				.WithMany(p => p.ProductsOnPallet)
				.HasForeignKey(pop => pop.PalletId)
				.IsRequired();

				entity.HasOne(pop => pop.Product)
				.WithMany()
				.HasForeignKey(pop => pop.ProductId)
				.IsRequired();

				entity.Property(p => p.BestBefore)
				.HasConversion(new ValueConverter<DateOnly?, DateTime?>(
					v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : null,
					v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null))
				.HasColumnType("date");
			});
			modelBuilder.Entity<Receipt>(entity =>
			 {
				 entity.HasKey(e => e.Id);
				 entity.Property(x => x.Id).ValueGeneratedOnAdd();

				 entity.HasMany(r => r.Pallets)
				 .WithOne(p => p.Receipt)
				 .HasForeignKey(p => p.ReceiptId)
				 .OnDelete(DeleteBehavior.Cascade);

			 });
			modelBuilder.Entity<ReversePicking>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();


			});
		}
	}
}
