using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;

namespace MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt
{
	public class AddPalletToReceiptCommandValidator : AbstractValidator<AddPalletToReceiptCommand>
	{
		public AddPalletToReceiptCommandValidator(IValidator<ProductOnPalletCreateDTO> productOnPalletValidator)
		{
			RuleFor(p => p.ReceiptId)
				.NotNull()
				.NotEmpty()
				.WithMessage("Receipt ID is required.");
			RuleFor(p => p.DTO.ReceiptNumber)
				.GreaterThan(0)
				.WithMessage("Receipt number is required.");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Pallet must contain at least one product.");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.Must(po => po.Select(p => p.ProductId)
				.Distinct().Count() <= 1)
				.WithMessage("A received pallet may contain only one product type.");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.Must(po => po.Select(po => po.BestBefore)
				.Distinct()
				.Count() <= 1)
				.WithMessage("Products on a pallet must have a single best-before date."); ;
			RuleForEach(p => p.DTO.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.DTO.ProductsOnPallet != null && p.DTO.ProductsOnPallet.Count > 0)
				.WithMessage("Pallet must contain at least one product.");
		}
	}
}
