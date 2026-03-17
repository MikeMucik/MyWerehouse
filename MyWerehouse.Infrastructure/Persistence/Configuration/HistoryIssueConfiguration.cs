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
	public class HistoryIssueConfiguration: IEntityTypeConfiguration<HistoryIssue>
	{
		public void Configure(EntityTypeBuilder<HistoryIssue> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();

			entity.Property(r => r.StatusAfter)
			.HasConversion<string>();
		}
	}
}
