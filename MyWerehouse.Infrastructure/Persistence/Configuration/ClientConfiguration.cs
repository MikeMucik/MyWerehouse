using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Infrastructure.Persistence.Configuration
{
	public class ClientConfiguration : IEntityTypeConfiguration<Client>
	{
		private readonly string? _providerName;
		public ClientConfiguration(string? providerName )
		{
			_providerName = providerName;
		}

		public void Configure(EntityTypeBuilder<Client> entity)
		{
			entity.HasKey(e => e.Id);
			entity.Property(x => x.Id).ValueGeneratedOnAdd();
						
			if (_providerName == "Microsoft.EntityFrameworkCore.SqlServer")
			{
				entity.Property(e => e.Name)
				.HasMaxLength(20)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.FullName)
				.HasMaxLength(DbLength.Name)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.Email)
				.HasMaxLength(DbLength.Email)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
				entity.Property(e => e.Description)
				.HasMaxLength(DbLength.Notes)
				.UseCollation("SQL_Latin1_General_CP1_CI_AS");
			}
		}
	}
}
