using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Validators
{
	public class CreateIssueDTOValidation : AbstractValidator<CreateIssueDTO>
	{
		public CreateIssueDTOValidation(IValidator<IssueItemDTO> itemValidator)
		{
			RuleFor(x => x.ClientId)
				.GreaterThan(0).WithMessage("Numer klienta wymagany");
			RuleFor(x => x.PerformedBy)
				.NotEmpty().WithMessage("Użytkownik wymagany");
			RuleFor(x => x.IssueDateTime)
				.GreaterThan(DateTime.MinValue).WithMessage("Nie prawidłowa data zamówienia");
			RuleForEach(x => x.Items).SetValidator(itemValidator);
			RuleFor(x => x.Items)
				.NotEmpty().WithMessage("Brak ilości i/lub towaru w zamówieniu");
		}
	}
}
