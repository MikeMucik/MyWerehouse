using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Invetories.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
	{
		public void Configure(EntityTypeBuilder<Inventory> entity)
		{
			entity.HasKey(e => e.ProductId);
			entity.Property(x => x.ProductId).ValueGeneratedNever();

			entity.HasOne(i => i.Product)
			.WithOne(p => p.InventoryItem)
			.HasForeignKey<Inventory>(i => i.ProductId)
			.OnDelete(DeleteBehavior.Restrict);//do przemyślenia
		}
	}
}
