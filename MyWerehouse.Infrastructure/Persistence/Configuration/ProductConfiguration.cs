using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Inventories.Models;
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
			if (_providerName == "Microsoft.EntityFrameworkCore.SqlServer")
			{
				entity.Property(e => e.Name)
				.HasMaxLength(DbLength.NameShort)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.SKU)
				.HasMaxLength(DbLength.SKU)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
			}

			entity.HasOne(p => p.Details)
			.WithOne(p => p.Product)
			.HasForeignKey<ProductDetail>(p => p.ProductId)
			.IsRequired()
			.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
