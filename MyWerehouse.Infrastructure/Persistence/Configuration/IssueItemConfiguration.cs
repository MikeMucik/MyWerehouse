using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class IssueItemConfiguration :IEntityTypeConfiguration<IssueItem>
	{
		public void Configure(EntityTypeBuilder<IssueItem> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();

			entity.HasOne(e => e.Issue)
				.WithMany(a => a.IssueItems)
				.HasForeignKey(a => a.IssueId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(e => e.Product)
				.WithMany()
				.HasForeignKey(e => e.ProductId)
				.OnDelete(DeleteBehavior.Restrict);

			//entity.HasOne(a => a.Issue)
			//	.WithMany(i => i.HistoryPickings)
			//	.HasForeignKey(h => h.IssueId)
			//	.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
