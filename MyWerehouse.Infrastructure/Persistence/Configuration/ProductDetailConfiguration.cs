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
	public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
	{
		public void Configure(EntityTypeBuilder<ProductDetail> entity)
		{
			entity.HasKey(e => e.ProductId);
			entity.Property(e => e.ProductId).ValueGeneratedNever();
			//może ograniczyć rozmiary kartonu
		}
	}
}
