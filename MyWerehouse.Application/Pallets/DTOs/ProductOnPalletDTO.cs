using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletDTO :IMapFrom<ProductOnPallet>
	{
		//public int Id { get; set; }
		public Guid ProductId { get; set; }		
		public Guid PalletId { get; set; }		
		public int Quantity { get; set; }
		public DateTime DateAdded { get; set; }
		public DateOnly? BestBefore { get; set; } // Może być null, jeśli produkt nie ma daty ważności
		public void Mapping(Profile profile)
		{
			//profile.CreateMap<ProductOnPalletDTO, ProductOnPallet>();
				//.ForMember(dest => dest.Id, opt => opt.Ignore());
			profile.CreateMap<ProductOnPallet, ProductOnPalletDTO>();
		}
	}
	public class ProductOnPalletDTOValidation : AbstractValidator<ProductOnPalletDTO>
	{
		public ProductOnPalletDTOValidation() 
		{
			RuleFor(pp => pp.ProductId)
				.NotEqual(Guid.Empty)
				.WithMessage("Produkt na palecie musi mieć numer produktu");
			RuleFor(pp => pp.Quantity)
				.GreaterThan(0)
				.WithMessage("Ilość produktu musi być większa od zera");
			RuleFor(pp => pp.DateAdded)
				.NotNull()
				.WithMessage("Produkt musi mieć datę przyjęcia");
			RuleFor(pp => pp.BestBefore)
				.GreaterThan(DateOnly.FromDateTime(DateTime.Now))
				.WithMessage("Data do spożycia musi być późniejsza niż data dzisiejsza")
				.When(pp => pp.BestBefore != null);			
		}
	}
}
