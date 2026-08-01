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
	public class CreateProductDTO : IMapFrom<Product>
	{
		public required string Name { get; init; }
		public required string SKU { get; init; }
		public int CategoryId { get; init; }
		public int CartonsPerPallet { get; init; }
		public int Length { get; init; } //cm
		public int Height { get; init; } //cm
		public int Width { get; init; } //cm
		public int Weight { get; init; } //grams
		public required string Description { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product, CreateProductDTO>()
				.ForMember(dest => dest.Length, opt => opt.MapFrom(static src => src.Details!.Length))
				.ForMember(dest => dest.Height, opt => opt.MapFrom(static src => src.Details!.Height))
				.ForMember(dest => dest.Width, opt => opt.MapFrom(static src => src.Details!.Width))
				.ForMember(dest => dest.Weight, opt => opt.MapFrom(static src => src.Details!.Weight))
				.ForMember(dest => dest.Description, opt => opt.MapFrom(static src => src.Details!.Description))
				.ReverseMap();
		}
	}
	public class AddProductDTOValidation : AbstractValidator<CreateProductDTO>
	{
		public AddProductDTOValidation()
		{
			RuleFor(p => p.Name).NotEmpty().WithMessage("Product name is required.");
			RuleFor(p => p.SKU).NotEmpty().WithMessage("Product SKU is required.");
			RuleFor(p => p.CartonsPerPallet).GreaterThan(0).WithMessage("Cartons per pallet must be greater than zero.");
			RuleFor(p => p.CategoryId).NotNull().WithMessage("Product category is required.");
			RuleFor(p => p.CategoryId).GreaterThan(0).WithMessage("Product category is required.");
			RuleFor(p => p.Height).GreaterThan(0).WithMessage("Product height must be greater than zero.");
			RuleFor(p => p.Width).GreaterThan(0).WithMessage("Product width must be greater than zero.");
			RuleFor(p => p.Weight).GreaterThan(0).WithMessage("Product weight must be greater than zero.");
			RuleFor(p => p.Length).GreaterThan(0).WithMessage("Product length must be greater than zero.");
		}
	}
}
