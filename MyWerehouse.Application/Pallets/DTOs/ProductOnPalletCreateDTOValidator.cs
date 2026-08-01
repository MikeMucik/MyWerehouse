using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletCreateDTOValidator : AbstractValidator<ProductOnPalletCreateDTO>
	{
		public ProductOnPalletCreateDTOValidator(IProductRepo productRepo, IDateTimeProvider dateTimeProvider)
		{
			RuleFor(pp => pp.ProductId)
				.NotEqual(Guid.Empty)
				.WithMessage("Product ID is required.");
			RuleFor(pp => pp.ProductId)
				.MustAsync(async (id, ct) => await productRepo.IsExistProduct(id))
				.WithMessage("The selected product does not exist.");
			RuleFor(pp => pp.Quantity)
				.GreaterThan(0)
				.WithMessage("Product quantity must be greater than zero.");
			RuleFor(pp => pp.DateAdded)
				.NotNull()
				.WithMessage("Product receipt date is required.");
			RuleFor(pp => pp.BestBefore)
				.GreaterThan(dateTimeProvider.Today)
				.WithMessage("Best-before date must be later than today.")
				.When(pp => pp.BestBefore != null);
		}
	}
}
