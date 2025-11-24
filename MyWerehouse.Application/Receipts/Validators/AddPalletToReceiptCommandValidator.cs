using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;

namespace MyWerehouse.Application.Receipts.Validators
{
	public class AddPalletToReceiptCommandValidator : AbstractValidator<AddPalletToReceiptCommand>
	{
		public AddPalletToReceiptCommandValidator(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{		
			RuleFor(p => p.DTO.Status)
				.NotEmpty()
				.WithMessage("Paleta musi mieć status")
				.When(p => !string.IsNullOrWhiteSpace(p.DTO.Id));
			RuleFor(p => p.DTO.DateReceived)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę przyjęcia")
				.When(p => !string.IsNullOrWhiteSpace(p.DTO.Id));
			RuleFor(p => p.DTO.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację początkową")
				.When(p => !string.IsNullOrWhiteSpace(p.DTO.Id));
			RuleFor(p => p.DTO.ReceiptId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć numer przyjęcia")
				.When(p => !string.IsNullOrWhiteSpace(p.DTO.Id));
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
