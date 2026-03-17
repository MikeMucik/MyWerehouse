using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class ProductOnPalletConfiguration : IEntityTypeConfiguration<ProductOnPallet>
	{
		public void Configure(EntityTypeBuilder<ProductOnPallet> entity)
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
		}
	}
}
