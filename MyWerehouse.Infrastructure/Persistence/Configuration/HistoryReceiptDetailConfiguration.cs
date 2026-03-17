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
	public class HistoryReceiptDetailConfiguration : IEntityTypeConfiguration<HistoryReceiptDetail>
	{
		public void Configure(EntityTypeBuilder<HistoryReceiptDetail> entity)
		{
			entity.HasOne(h => h.HistoryReceipt)
				.WithMany(hd => hd.Details)
				.HasForeignKey(h => h.HistoryReceiptId);
		}
	}
}
