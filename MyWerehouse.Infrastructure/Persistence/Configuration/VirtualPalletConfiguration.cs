using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class VirtualPalletConfiguration : IEntityTypeConfiguration<VirtualPallet>
	{
		public void Configure(EntityTypeBuilder<VirtualPallet> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(entity => entity.Id).ValueGeneratedNever();
			//entity.Property(entity => entity.Id).ValueGeneratedOnAdd();

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
		}
	}
}
