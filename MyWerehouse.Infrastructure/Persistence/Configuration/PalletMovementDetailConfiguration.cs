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
	public class PalletMovementDetailConfiguration : IEntityTypeConfiguration<HistoryPalletDetail>
	{
		public void Configure(EntityTypeBuilder<HistoryPalletDetail> entity)
		{

			entity.HasOne(md => md.HistoryPallet)
			.WithMany(pm => pm.HistoryPalletDetails)
			.HasForeignKey(md => md.HistoryPalletId);
		}
	}
}
