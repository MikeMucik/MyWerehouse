using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Validators
{
	public class UpdatePalletCommandValidation :AbstractValidator<UpdatePalletCommand>
	{
		public UpdatePalletCommandValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.UpdatingPallet.Status)
				.NotEmpty()
				.WithMessage("Paleta musi mieć status");
			RuleFor(p => p.UpdatingPallet.DateReceived)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę utworzenia");
			RuleFor(p => p.UpdatingPallet.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację");
			RuleFor(p => p.UpdatingPallet.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleForEach(p => p.UpdatingPallet.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.UpdatingPallet.ProductsOnPallet != null && p.UpdatingPallet.ProductsOnPallet.Count > 0);
		}
	}
}
