using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Validators
{
	public class CreateIssueDTOValidator : AbstractValidator<CreateIssueCommand>
	{
		public CreateIssueDTOValidator(IValidator<IssueItemDTO> itemValidator)
		{
			RuleFor(x => x.DTO.ClientId)
				.GreaterThan(0).WithMessage("Numer klienta wymagany");
			RuleFor(x => x.DTO.PerformedBy)
				.NotEmpty().WithMessage("Użytkownik wymagany");
			RuleForEach(x => x.DTO.Items).SetValidator(itemValidator);
			RuleFor(x => x.DTO.Items)
				.NotEmpty().WithMessage("Brak ilości i/lub towaru w zamówieniu");
		}
	}
}
