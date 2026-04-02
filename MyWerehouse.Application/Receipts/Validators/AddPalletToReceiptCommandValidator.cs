using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;

namespace MyWerehouse.Application.Receipts.Validators
{
	public class AddPalletToReceiptCommandValidator : AbstractValidator<AddPalletToReceiptCommand>
	{
		public AddPalletToReceiptCommandValidator(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.ReceiptId)
				.NotNull()
				.WithMessage("Paleta musi mieć numer przyjęcia");
			RuleFor(p => p.DTO.ReceiptNumber)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć numer przyjęcia");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.Must(po => po.Select(p => p.ProductId)
				.Distinct().Count() <= 1)
				.WithMessage("Paleta przyjmowana może mieć tylko jeden rodzaj produktu");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.Must(po => po.Select(po => po.BestBefore)
				.Distinct()
				.Count() <= 1)
				.WithMessage("Produkt musi mieć jedną datą BestBefore"); ;
			RuleForEach(p => p.DTO.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.DTO.ProductsOnPallet != null && p.DTO.ProductsOnPallet.Count > 0)
				.WithMessage("Paleta musi zawierać towar/y"); 
		}	
	}
}
