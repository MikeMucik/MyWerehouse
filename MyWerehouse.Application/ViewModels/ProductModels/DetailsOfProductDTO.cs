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
	public class DetailsOfProductDTO : IMapFrom<Product>
	{
		public Guid Id { get; init; }//
		public string Name { get; init; }
		public string CategoryName { get; init; }
		public int CategoryId { get; init; }
		public int CartonsPerPallet { get; init; }
		public int Length { get; init; } //cm
		public int Height { get; init; } //cm
		public int Width { get; init; } //cm
		public int Weight { get; init; } //kg
		public string Description { get; init; }
		public DateTime AddedItemAd { get; init; } = DateTime.Now;
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product, DetailsOfProductDTO>()
				.ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Details.Length))
				.ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Details.Height))
				.ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Details.Width))
				.ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Details.Weight))
				.ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
				.ForMember(dest => dest.Description, opt=>opt.MapFrom(src=>src.Details.Description))
				.ReverseMap()
				;
		}
	}
}
