using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Validators
{
	public class ProductOnPalletDTOValidator : AbstractValidator<ProductOnPalletDTO>
	{
		public ProductOnPalletDTOValidator()
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
