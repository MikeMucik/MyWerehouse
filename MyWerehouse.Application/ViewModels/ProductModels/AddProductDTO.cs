using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class AddProductDTO :IMapFrom<Product>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string SKU { get; set; }
		//public string Category { get; set; }
		public int CategoryId { get; set; }
		public bool? IsDeleted { get; set; }
		public int CartonsPerPallet { get; set; }
		//public int? DetailsId { get; set; }// Update !!!
		public int Length { get; set; } //cm
		public int Height { get; set; } //cm
		public int Width { get; set; } //cm
		public int Weight { get; set; } //kg
		public string Description { get; set; }
		public DateTime AddedItemAd { get; set; } = DateTime.Now;
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product, AddProductDTO>()
				.ForMember(dest=>dest.Length, opt=>opt.MapFrom(src=>src.Details.Length))
				.ForMember(dest=>dest.Height, opt=>opt.MapFrom(src=>src.Details.Height))
				.ForMember(dest=>dest.Width, opt=>opt.MapFrom(src=>src.Details.Width))
				.ForMember(dest=>dest.Weight, opt=>opt.MapFrom(src=>src.Details.Weight))
				//.ForMember(dest=>dest.Category, opt=>opt.MapFrom(src=>src.Category.Name))
				.ForMember(dest=>dest.Description, opt=>opt.MapFrom(src=>src.Details.Description))
				//.ForMember(dest=>dest.DetailsId, opt=>opt.MapFrom(src=>src.Details.Id))
				.ForMember(dest=>dest.AddedItemAd, opt=>opt.MapFrom(src=>src.AddedItemAd))
				.ReverseMap();
		}
		
	}
	public class AddProductDTOValidation : AbstractValidator<AddProductDTO>
	{
		public AddProductDTOValidation() 
		{
			RuleFor(p => p.Name).NotNull().WithMessage("Uzupełnij dane - nazwa");
			RuleFor(p => p.SKU).NotNull().WithMessage("Uzupełnij dane - SKU");
			RuleFor(p => p.CategoryId).NotNull().WithMessage("Uzupełnij dane - kategoria");
			RuleFor(p => p.Height).GreaterThan(0).WithMessage("Uzupełnij dane - wysokość");
			RuleFor(p => p.Width).GreaterThan(0).WithMessage("Uzupełnij dane - szerokość");
			RuleFor(p => p.Weight).GreaterThan(0).WithMessage("Uzupełnij dane - waga");
			RuleFor(p => p.Length).GreaterThan(0).WithMessage("Uzupełnij dane - długość");
		}
	}
}
