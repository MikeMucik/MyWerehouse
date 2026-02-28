using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.DTOs;

namespace MyWerehouse.Application.Receipts.Validators
{
	public class CreatePalletReceiptDTOValidation : AbstractValidator<CreatePalletReceiptDTO>
	{
		public CreatePalletReceiptDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.Status)
				.NotEmpty()
				.WithMessage("Paleta musi mieć status")
				.When(p => !string.IsNullOrWhiteSpace(p.Id));
			RuleFor(p => p.DateReceived)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę przyjęcia")
				.When(p => !string.IsNullOrWhiteSpace(p.Id));
			RuleFor(p => p.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację początkową")
				.When(p => !string.IsNullOrWhiteSpace(p.Id));
			RuleFor(p => p.ReceiptNumber)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć numer przyjęcia")
				.When(p => !string.IsNullOrWhiteSpace(p.Id));
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleFor(p => p.ProductsOnPallet)
				.Must(po => po.Select(p => p.ProductId)
				.Distinct().Count() <= 1)
				.WithMessage("Paleta przyjmowana może mieć tylko jeden rodzaj produktu");
			RuleFor(p => p.ProductsOnPallet)
				.Must(po => po.Select(po => po.BestBefore)
				.Distinct()
				.Count() <= 1)
				.WithMessage("Produkt musi mieć jedną datą BestBefore"); ;
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
		}
	}
}
