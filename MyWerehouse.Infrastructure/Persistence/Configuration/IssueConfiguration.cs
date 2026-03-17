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
	public class IssueConfiguration : IEntityTypeConfiguration<Issue>
	{
		public void Configure(EntityTypeBuilder<Issue> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedNever();
			//entity.Property(x => x.IssueNumber).ValueGeneratedOnAdd();

			//entity.HasOne(a => a.Issue)
			//	.WithMany(i => i.PickingTasks)
			//	.HasForeignKey(a => a.IssueId)
			//	.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
