using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Validators
{
	public class UpdateIssueDTOValidation : AbstractValidator<UpdateIssueDTO>
	{
		public UpdateIssueDTOValidation(IValidator<IssueItemDTO> itemValidator)
		{
			RuleFor(x => x.Id).NotEmpty()
				.WithMessage("Numer zamówienia wymagany");
			RuleFor(x => x.ClientId)
				.GreaterThan(0).WithMessage("Numer Klienta wymagany");
			RuleFor(x => x.DateToSend)
				.GreaterThan(DateTime.MinValue).WithMessage("Nie prawidłowa data zamówienia");
			RuleForEach(x => x.Items).SetValidator(itemValidator);
			RuleFor(x => x.Items)
				.NotEmpty().WithMessage("Brak ilości i/lub towaru w zamówieniu");
		}
	}
}
