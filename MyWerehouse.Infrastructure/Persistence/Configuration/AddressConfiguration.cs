using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Common.ValueObject;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class AddressConfiguration : IEntityTypeConfiguration<Address>
	{
		private readonly string? _providerName;

		public AddressConfiguration(string? providerName)
		{
			_providerName = providerName;
		}

		public void Configure(EntityTypeBuilder<Address> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();

			if (_providerName == "Microsoft.EntityFrameworkCore.SqlServer")
			{
				entity.Property(e => e.Country)
				.HasMaxLength(DbLength.Country)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.City)
				.HasMaxLength(DbLength.City)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.Region)
				.HasMaxLength(DbLength.Country)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.StreetName)
				.HasMaxLength(DbLength.Street)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.PostalCode)
				.HasMaxLength(DbLength.PostalCode)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.StreetNumber)
				.HasMaxLength(DbLength.StreetNumber)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
			}
		}
	}
}
