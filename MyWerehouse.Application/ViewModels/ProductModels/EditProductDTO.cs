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
	public class EditProductDTO :IMapFrom<Product>
	{		
		public string Name { get; init; }
		public string SKU { get; init; }
		public int CategoryId { get; init; }
		public bool? IsDeleted { get; init; } = false;
		public int CartonsPerPallet { get; init; }
		public int Length { get; init; } //cm
		public int Height { get; init; } //cm
		public int Width { get; init; } //cm
		public int Weight { get; init; } //kg
		public string Description { get; init; }
		public DateTime AddedItemAd { get; init; } = DateTime.Now;
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product, EditProductDTO >()
				.ForMember(dest => dest.Length, opt => opt.MapFrom(static src => src.Details.Length))
				.ForMember(dest => dest.Height, opt => opt.MapFrom(static src => src.Details.Height))
				.ForMember(dest => dest.Width, opt => opt.MapFrom(static src => src.Details.Width))
				.ForMember(dest => dest.Weight, opt => opt.MapFrom(static src => src.Details.Weight))
				.ForMember(dest => dest.Description, opt => opt.MapFrom(static src => src.Details.Description))
				.ReverseMap();
		}		
	}
	public class AddProductDTOValidation : AbstractValidator<EditProductDTO>
	{
		public AddProductDTOValidation() 
		{
			RuleFor(p => p.Name).NotNull().WithMessage("Uzupełnij dane - nazwa");
			RuleFor(p => p.SKU).NotNull().WithMessage("Uzupełnij dane - SKU");
			RuleFor(p => p.CartonsPerPallet).GreaterThan(0).WithMessage("Ilość kartonów na paletę musi być więcej niż 0.");
			RuleFor(p => p.CategoryId).NotNull().WithMessage("Uzupełnij dane - kategoria");
			RuleFor(p => p.CategoryId).GreaterThan(0).WithMessage("Uzupełnij dane - kategoria");
			RuleFor(p => p.Height).GreaterThan(0).WithMessage("Uzupełnij dane - wysokość");
			RuleFor(p => p.Width).GreaterThan(0).WithMessage("Uzupełnij dane - szerokość");
			RuleFor(p => p.Weight).GreaterThan(0).WithMessage("Uzupełnij dane - waga");
			RuleFor(p => p.Length).GreaterThan(0).WithMessage("Uzupełnij dane - długość");
		}
	}
}
