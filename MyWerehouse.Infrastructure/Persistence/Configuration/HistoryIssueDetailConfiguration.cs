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
	public class HistoryIssueDetailConfiguration :IEntityTypeConfiguration<HistoryIssueDetail>
	{
		public void Configure(EntityTypeBuilder<HistoryIssueDetail> entity)
		{
			entity.HasOne(h => h.HistoryIssue)
				.WithMany(hd => hd.Details)
				.HasForeignKey(h => h.HistoryIssueId);
		}


	}
}
