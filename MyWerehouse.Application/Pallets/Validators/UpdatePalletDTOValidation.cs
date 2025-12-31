using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Validators
{
	public class UpdatePalletDTOValidation : AbstractValidator<UpdatePalletDTO>
	{
		public UpdatePalletDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.Status)
				.NotEmpty()
				.WithMessage("Paleta musi mieć status");
			RuleFor(p => p.DateReceived)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę utworzenia");
			RuleFor(p => p.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
		}
	}
}
