using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;

namespace MyWerehouse.Application.Receipts.Validators
{
	public class UpdateReceiptCommandValidation : AbstractValidator<UpdateReceiptCommand>
	{
		public UpdateReceiptCommandValidation(IValidator<UpdatePalletDTO> palletValidator)
		{
			RuleFor(r => r.DTO.ReceiptNumber)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć swój numer.");
			RuleFor(r => r.DTO.ClientId)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć numer klienta.");
			RuleFor(r => r.DTO.ReceiptDateTime)
				.NotEqual(default(DateTime))
				.WithMessage("Przyjęcie musi mieć datę.");
			RuleFor(r => r.DTO.Pallets)
				.NotEmpty()
				.WithMessage("Przyjęcie musi zawierać przyjęte palety.");
			RuleFor(r => r.UserId)
				.NotEmpty()
				.WithMessage("Przyjecie musi zawierać użytkownika.");
			RuleForEach(p => p.DTO.Pallets)
				.SetValidator(palletValidator)
				.When(p => p.DTO.Pallets != null && p.DTO.Pallets.Any());
		}
	}
}
