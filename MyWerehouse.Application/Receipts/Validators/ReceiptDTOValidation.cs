using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.ViewModels.PalletModels;

namespace MyWerehouse.Application.Receipts.Validators
{
	public class ReceiptDTOValidation : AbstractValidator<ReceiptDTO>
	{
		public ReceiptDTOValidation(IValidator<UpdatePalletDTO> palletValidator)
		{
			RuleFor(r => r.Id)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć swój numer.");
			RuleFor(r => r.ClientId)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć numer klienta");
			RuleFor(r => r.ReceiptDateTime)
				.NotEqual(default(DateTime))
				.WithMessage("Przyjęcie musi mieć datę.");
			RuleFor(r => r.Pallets)
				.NotEmpty()
				.WithMessage("Przyjęcie musi zawierać przyjęte palety");
			//RuleFor(r => r.PerformedBy)
			RuleForEach(p => p.Pallets)
				.SetValidator(palletValidator)
				.When(p => p.Pallets != null && p.Pallets.Any());
		}
	}
}
