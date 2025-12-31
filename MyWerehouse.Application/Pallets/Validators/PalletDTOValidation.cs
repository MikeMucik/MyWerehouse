using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Pallets.Validators
{
	public class PalletDTOValidation : AbstractValidator<PalletDTO>
	{
		public PalletDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.ProductsOnPallet)
				.NotNull()
				.WithMessage("Paleta musi zawierać produkty");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);			
		}
	}
}
