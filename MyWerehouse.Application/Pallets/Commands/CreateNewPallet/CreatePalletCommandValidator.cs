using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Commands.AddPalletToReceipt;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreatePalletCommandValidator : AbstractValidator<CreatePalletCommand>
	{
		public CreatePalletCommandValidator(IValidator<ProductOnPalletCreateDTO> productOnPalletValidator, ILocationRepo locationRepo)
		{
			RuleFor(p => p.DTO)
				.NotNull()
				.WithMessage("Pallet data is required.");
			RuleFor(p => p.DTO.ProductsOnPallet)
				.NotNull()
				.WithMessage("Pallet must contain at least one product.");
			RuleFor(p => p.RampNumber)
				.GreaterThan(0)
				.WithMessage("Pallet location is required.");
			RuleFor(p => p.RampNumber)
				.MustAsync(async (id, ct) => await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("The selected ramp does not exist.");
			RuleForEach(p => p.DTO.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.DTO.ProductsOnPallet != null && p.DTO.ProductsOnPallet.Count > 0);
		}
	}
}
