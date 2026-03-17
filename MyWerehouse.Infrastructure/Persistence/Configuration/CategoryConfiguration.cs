using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class CategoryConfiguration :IEntityTypeConfiguration<Category>
	{
		private readonly string? _providerName;
		public CategoryConfiguration(string? providerName)
		{
			_providerName = providerName;
		}
		public void Configure(EntityTypeBuilder<Category> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();
			if (_providerName == "Microsoft.EntityFrameworkCore.SqlServer")
			{
				entity.Property(e => e.Name)
				.HasMaxLength(DbLength.NameShort)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
			}
		}
	}
}
