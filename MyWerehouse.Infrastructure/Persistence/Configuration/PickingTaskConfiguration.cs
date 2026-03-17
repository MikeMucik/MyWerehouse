using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class PickingTaskConfiguration:IEntityTypeConfiguration<PickingTask>
	{		
		public void Configure(EntityTypeBuilder<PickingTask> entity)
		{

			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id).ValueGeneratedNever();

			entity.Property(a => a.PickingStatus)
			.HasConversion<string>();

			entity.HasOne(a => a.VirtualPallet)
				.WithMany(p => p.PickingTasks)
				.HasForeignKey(a => a.VirtualPalletId);

			entity.HasOne(a => a.Issue)
				 .WithMany(a => a.PickingTasks)
				 .HasForeignKey(a => a.IssueId);

			entity.HasOne(a => a.PickingPallet)
			.WithMany()
			.HasForeignKey(a => a.PickingPalletId)
			.IsRequired(false)
			.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
