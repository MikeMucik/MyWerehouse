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
		public Guid ProductId { get; init; }
		public string ProductSKU { get; init; }//
		public string ProductName { get; init; }//
		public Guid PalletId { get; init; }
		public int Quantity { get; init; }
		public DateTime DateAdded { get; init; }
		public DateOnly? BestBefore { get; init; } // Może być null, jeśli produkt nie ma daty ważności
		public void Mapping(Profile profile)
		{
			profile.CreateMap<ProductOnPallet, ProductOnPalletDTO>()
				.ForMember(dest=>dest.ProductSKU, opt=>opt.MapFrom(src=>src.Product.SKU))
				.ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));
		}
	}
}