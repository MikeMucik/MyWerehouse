using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class ReversePickingConfiguration : IEntityTypeConfiguration<ReversePicking>
	{
		public void Configure(EntityTypeBuilder<ReversePicking> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedNever();
		}
	}
}
