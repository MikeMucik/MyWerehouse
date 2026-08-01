using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Queries.GetReceiptById;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class EditPalletInReceiptDTOValidator : AbstractValidator<EditPalletInReceiptDTO>
	{
		public EditPalletInReceiptDTOValidator(IValidator<ProductOnPalletCreateDTO> productOnPalletValidator)
		{
			RuleFor(p => p.Status)
				.NotEmpty()
				.WithMessage("Pallet status is required.");
			RuleFor(p => p.DateReceived)
				.NotEmpty()
				.WithMessage("Pallet receipt date is required.");
			RuleFor(p => p.LocationId)
				.GreaterThan(0)
				.WithMessage("Pallet location is required.");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Pallet must contain at least one product.");
			RuleFor(p => p.ProductsOnPallet)
				.Must(a => a.Count() == 1)
				.WithMessage("A received pallet may contain only one product type.");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
		}
	}
}
