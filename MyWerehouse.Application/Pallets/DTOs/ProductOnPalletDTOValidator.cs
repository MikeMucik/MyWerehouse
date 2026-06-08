using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletDTOValidator : AbstractValidator<ProductOnPalletDTO>
	{
		public ProductOnPalletDTOValidator(IProductRepo productRepo)
		{
			RuleFor(pp => pp.ProductId)
				.NotEqual(Guid.Empty)
				.WithMessage("Produkt na palecie musi mieć numer produktu");
			RuleFor(pp => pp.ProductId)
				.MustAsync(async (id, ct) => await productRepo.IsExistProduct(id))
				.WithMessage("Wybrany product nie istnieje.");
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
