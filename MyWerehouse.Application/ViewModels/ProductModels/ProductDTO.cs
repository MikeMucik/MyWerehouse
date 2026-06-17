using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class ProductDTO : IMapFrom<Product>
	{
		public Guid Id { get; init; }
		public string Name { get; init; }
		public string SKU { get; init; }
		public string Category { get; init; }
		public bool IsDeleted { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product, ProductDTO>()
				.ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.Name));
		}
	}
}
