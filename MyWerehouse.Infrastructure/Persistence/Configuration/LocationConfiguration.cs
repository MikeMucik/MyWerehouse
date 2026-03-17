using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class LocationConfiguration : IEntityTypeConfiguration<Location>
	{
		public void Configure(EntityTypeBuilder<Location> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();
		}
	}
}
