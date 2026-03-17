using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class PalletMovementDetailConfiguration : IEntityTypeConfiguration<PalletMovementDetail>
	{
		public void Configure(EntityTypeBuilder<PalletMovementDetail> entity)
		{

			entity.HasOne(md => md.PalletMovement)
			.WithMany(pm => pm.PalletMovementDetails)
			.HasForeignKey(md => md.PalletMovementId);
		}
	}
}
