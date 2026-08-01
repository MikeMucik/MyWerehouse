using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class ReversePickingConfiguration : IEntityTypeConfiguration<ReversePickingTask>
	{
		public void Configure(EntityTypeBuilder<ReversePickingTask> entity)
		{
			entity.HasKey(e => e.Id);
			entity.HasOne(e => e.PickingTask)
				.WithOne()
				.HasForeignKey<ReversePickingTask>(e => e.PickingTaskId);
			entity.Property(x => x.Id).ValueGeneratedNever();
		}
	}
}
