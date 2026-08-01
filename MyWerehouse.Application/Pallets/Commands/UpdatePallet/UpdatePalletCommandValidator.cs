using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Commands.UpdatePallet
{
	public class UpdatePalletCommandValidator :AbstractValidator<UpdatePalletCommand>
	{
		public UpdatePalletCommandValidator(IValidator<ProductOnPalletCreateDTO> productOnPalletValidator)
		{
			RuleFor(p => p.UpdatingPallet.Status)
				.NotEmpty()
				.WithMessage("Pallet status is required.");
			RuleFor(p => p.UpdatingPallet.LocationId)
				.GreaterThan(0)
				.WithMessage("Pallet location is required.");
			RuleFor(p => p.UpdatingPallet.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Pallet must contain at least one product.");
			RuleForEach(p => p.UpdatingPallet.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.UpdatingPallet.ProductsOnPallet != null && p.UpdatingPallet.ProductsOnPallet.Count > 0);
		}
	}
}
