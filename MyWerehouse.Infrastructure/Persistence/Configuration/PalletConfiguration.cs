using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class PalletConfiguration :  IEntityTypeConfiguration<Pallet>
	{
		public void Configure(EntityTypeBuilder<Pallet> entity)
		{
			entity.HasKey(p => p.Id);
			entity.Property(p => p.Id)
				.IsRequired()
				.HasMaxLength(10);

			entity.Property(p => p.Status)
			.HasConversion<string>();

			entity.HasMany(p => p.ProductsOnPallet)
				.WithOne(a => a.Pallet)
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
		}
	}
}
