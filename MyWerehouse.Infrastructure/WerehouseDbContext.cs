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
		public DbSet<Inventory> Inventory { get; set; }
		public DbSet<Issue> Issues { get; set; }
		public DbSet<Location> Locations { get; set; }
		public DbSet<Pallet> Pallets { get; set; }
		public DbSet<PalletMovement> PalletMovement { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<ProductDetails> ProductDetails { get; set; }
		public DbSet<ProductOnPallet> ProductOnPallet { get; set; }
		public DbSet<Receipt> Receipts { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ProductOnPallet>()
				.Property(p => p.BestBefore)
				.HasConversion(new ValueConverter<DateOnly?, DateTime?>(
					v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : null,
					v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null))
				.HasColumnType("date");

			modelBuilder.Entity<Pallet>()
				.Property(p => p.Status)
				.HasConversion<string>();

			modelBuilder.Entity<PalletMovement>()
				.Property(m => m.Reason)
				.HasConversion<string>();

			modelBuilder.Entity<PalletMovement>()
				.HasOne(pm => pm.Pallet)
				.WithMany(p => p.PalletMovements) // Jeśli masz to w modelu
				.HasForeignKey(pm => pm.PalletId)
				.OnDelete(DeleteBehavior.Restrict); // ⛔️ NIE rób Cascade

			modelBuilder.Entity<Product>()
				.HasOne(p => p.Details)
				.WithOne(p => p.Product)
				.HasForeignKey<ProductDetails>(p => p.ProductId);			
		}
	}
}
