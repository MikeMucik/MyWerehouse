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
		public Guid Id { get; set; }//
		public string Name { get; set; }
		public string CategoryName { get; set; }
		public int CategoryId { get; set; }
		public int CartonsPerPallet { get; set; }
		public int Length { get; set; } //cm
		public int Height { get; set; } //cm
		public int Width { get; set; } //cm
		public int Weight { get; set; } //kg
		public string Description { get; set; }
		public DateTime AddedItemAd { get; set; } = DateTime.Now;
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
