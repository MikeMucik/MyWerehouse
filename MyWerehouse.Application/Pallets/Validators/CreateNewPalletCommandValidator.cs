using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;

namespace MyWerehouse.Application.Pallets.Validators
{
	public class CreateNewPalletCommandValidator : AbstractValidator<CreateNewPalletCommand>
	{
		public CreateNewPalletCommandValidator(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.DTO)
				.NotNull()
				.WithMessage("Dane palety są wymagane");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.NotNull()
				.WithMessage("Paleta musi zawierać produkty");
			RuleForEach(p => p.DTO.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.DTO.ProductsOnPallet != null && p.DTO.ProductsOnPallet.Count > 0);
		}
	}
}
