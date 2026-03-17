using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
	{
		public void Configure(EntityTypeBuilder<Receipt> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedNever();

			entity.HasMany(r => r.Pallets)
			.WithOne(p => p.Receipt)
			.HasForeignKey(p => p.ReceiptId)
			.OnDelete(DeleteBehavior.Cascade);
		}
	}
}
