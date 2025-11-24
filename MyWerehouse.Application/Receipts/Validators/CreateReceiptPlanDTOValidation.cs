using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Receipts.DTOs;

namespace MyWerehouse.Application.Receipts.Validators
{
	public class CreateReceiptPlanDTOValidation : AbstractValidator<CreateReceiptPlanDTO>
	{
		public CreateReceiptPlanDTOValidation()
		{
			RuleFor(x => x.ClientId)
				.GreaterThan(0).WithMessage("Numer klienta wymagany");
			RuleFor(x => x.PerformedBy)
				.NotEmpty().WithMessage("Użytkownik wymagany");
		}
	}
}
