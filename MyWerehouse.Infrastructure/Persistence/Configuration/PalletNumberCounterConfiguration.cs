using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Common.ValueObject;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class PalletNumberCounterConfiguration :IEntityTypeConfiguration<PalletNumberCounter>
	{		
		public void Configure(EntityTypeBuilder<PalletNumberCounter> entity)
		{
			entity.HasKey(n => n.Name);

			entity.Property(n => n.Name)
				.HasMaxLength(30);

			entity.HasData(new PalletNumberCounter
			{
				Name = "Pallet",
				NextNumber = 1
			});

		}
	}
}
