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
	public class HistoryPickingConfiguration : IEntityTypeConfiguration<HistoryPicking>
	{
		public void Configure(EntityTypeBuilder<HistoryPicking> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();

			entity.Property(a => a.StatusBefore)
			.HasConversion<string>();

			entity.Property(a => a.StatusAfter)
			.HasConversion<string>();
			//// Indeks dla szybkiego wyszukiwania
			entity.HasIndex(x => x.PalletId);
			entity.HasIndex(x => x.PickingTaskId);
			entity.HasIndex(x => x.IssueId);
			entity.HasIndex(h => h.DateTime);
		}
	}
}
