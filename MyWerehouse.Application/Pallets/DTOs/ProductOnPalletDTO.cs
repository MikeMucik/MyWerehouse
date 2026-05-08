using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletDTO : IMapFrom<ProductOnPallet>
	{
		public Guid ProductId { get; set; }
		public Guid PalletId { get; set; }
		public int Quantity { get; set; }
		public DateTime DateAdded { get; set; }
		public DateOnly? BestBefore { get; set; } // Może być null, jeśli produkt nie ma daty ważności
		public void Mapping(Profile profile)
		{
			profile.CreateMap<ProductOnPallet, ProductOnPalletDTO>();
		}
	}
}