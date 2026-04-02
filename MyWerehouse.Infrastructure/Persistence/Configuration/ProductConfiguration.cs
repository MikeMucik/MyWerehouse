using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class ProductConfiguration : IEntityTypeConfiguration<Product>
	{
		private readonly string? _providerName;
		public ProductConfiguration(string? providerName)
		{
			_providerName = providerName;
		}

		public void Configure(EntityTypeBuilder<Product> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(e=>e.Id).ValueGeneratedNever();
			//entity.Property(e => e.Id).ValueGeneratedOnAdd();
			if (_providerName == "Microsoft.EntityFrameworkCore.SqlServer")
			{
				entity.Property(e => e.Name)
				.HasMaxLength(DbLength.NameShort)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.SKU)
				.HasMaxLength(DbLength.SKU)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
			}

			entity.HasOne(pm => pm.InventoryItem)
			.WithOne(i => i.Product)
			.HasForeignKey<Inventory>(i => i.ProductId);

			//entity.OwnsOne(p=>p.Details)


			entity.HasOne(p => p.Details)
			.WithOne(p => p.Product)
			.HasForeignKey<ProductDetail>(p => p.ProductId)
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
