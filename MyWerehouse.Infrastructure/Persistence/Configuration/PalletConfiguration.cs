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
	public class PalletConfiguration : IEntityTypeConfiguration<Pallet>
	{
		public void Configure(EntityTypeBuilder<Pallet> entity)
		{
			entity.HasKey(p => p.Id);
			entity.Property(p => p.Id)
				.IsRequired()
				.HasMaxLength(10);

			entity.HasIndex(p => p.PalletNumber)
				.IsUnique();

			entity.Property(p => p.Status)
			.HasConversion<string>();

			entity.HasMany(p => p.ProductsOnPallet)
				.WithOne(a => a.Pallet)
				.HasForeignKey(pop => pop.PalletId)
				.IsRequired()
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasMany(p => p.PalletHistory)
				.WithOne()
				.HasForeignKey(h => h.PalletId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(p => p.Issue)
				.WithMany(i => i.Pallets)
				.HasForeignKey(p => p.IssueId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.Property(e => e.RowVersion)
			  .IsRowVersion()  // Important: marks the field as Timestamp/RowVersion
			  .HasColumnType("rowversion")  // For SQL Server; adjust for other databases
			  .IsRequired(false);  // Optional, but Timestamp is usually nullable
		}
	}
}
