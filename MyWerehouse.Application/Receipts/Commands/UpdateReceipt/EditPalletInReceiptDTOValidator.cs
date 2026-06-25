using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class EditPalletInReceiptDTOValidator : AbstractValidator<EditPalletInReceiptDTO>
	{
		public EditPalletInReceiptDTOValidator(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.Status)
				.NotEmpty()
				.WithMessage("Paleta musi mieć status.");
			RuleFor(p => p.DateReceived)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę utworzenia.");
			RuleFor(p => p.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację.");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar.");
			RuleFor(p => p.ProductsOnPallet)
				.Must(a => a.Count() == 1)
				.WithMessage("Paleta w przyjęciu może mieć tylko jeden rodzaj towaru.");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
		}
	}
}
