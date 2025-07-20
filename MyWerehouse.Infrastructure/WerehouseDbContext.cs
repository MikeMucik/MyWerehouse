using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure
{
	public class WerehouseDbContext : IdentityDbContext
	{
		public WerehouseDbContext(DbContextOptions<WerehouseDbContext> options) : base(options) { }
		public DbSet<Address> Addresses { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Client> Clients { get; set; }
		public DbSet<HistoryIssue> HistoryIssues { get; set; }
		public DbSet<HistoryIssueDetail> HistoryIssuesDetail { get; set; }
		public DbSet<Inventory> Inventories { get; set; }
		public DbSet<Issue> Issues { get; set; }
		public DbSet<Location> Locations { get; set; }
		public DbSet<Pallet> Pallets { get; set; }
		public DbSet<PalletMovement> PalletMovements { get; set; }
		public DbSet<PalletMovementDetail> PalletMovementDetails { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<ProductDetail> ProductDetails { get; set; }
		public DbSet<ProductOnPallet> ProductOnPallet { get; set; }
		public DbSet<Receipt> Receipts { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Address>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.Property(x => x.Id).ValueGeneratedOnAdd();
			});
			modelBuilder.Entity<Category>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
			});
			modelBuilder.Entity<Client>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
			});
			modelBuilder.Entity<HistoryIssue>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

					entity.HasOne(e => e.Issue)
						.WithMany(p => p.HistoryIssues)
						.HasForeignKey(e => e.IssueId)
						.IsRequired();

				entity.Property(r => r.Status)
				.HasConversion<string>();
			});
			modelBuilder.Entity<HistoryIssueDetail>(entity =>
			{
				entity.HasOne(h => h.HistoryIssue)
				.WithMany(hd => hd.Details)
				.HasForeignKey(h => h.HistoryIssueId);
			});
			modelBuilder.Entity<Inventory>(entity =>
			{
				entity.HasKey(e => e.ProductId);
				entity.Property(x => x.ProductId).ValueGeneratedNever();

				entity.HasOne(i => i.Product)
				.WithOne(p => p.InventoryItem)
				.HasForeignKey<Inventory>(i => i.ProductId);
			});
			modelBuilder.Entity<Issue>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();
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
				.WithOne()
				.HasForeignKey(pop => pop.PalletId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<PalletMovement>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(x => x.Id).ValueGeneratedOnAdd();

				entity.HasOne(pm => pm.Pallet)
					.WithMany(p => p.PalletMovements)
					.HasForeignKey(pm => pm.PalletId)
					.IsRequired()
					//.OnDelete(DeleteBehavior.Restrict); // ⛔️ NIE rób Cascade
					.OnDelete(DeleteBehavior.Cascade);
				// Zrób cascade bo to tyczy się tylko jak przyjęcie jeszcze nie zweryfikowane
				entity.Property(m => m.Reason)
				.HasConversion<string>();
			});
			modelBuilder.Entity<PalletMovementDetail>(entity =>
			{
				entity.HasOne(md => md.PalletMovement)
				.WithMany(pm => pm.PalletMovementDetails)
				.HasForeignKey(md => md.PalletMovementId);
			});
			modelBuilder.Entity<Product>(entity =>
			{
				entity.HasKey(e => e.Id);
				entity.Property(e => e.Id).ValueGeneratedOnAdd();

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
		}
	}
}
