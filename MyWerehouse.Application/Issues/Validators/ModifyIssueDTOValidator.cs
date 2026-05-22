using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.Commands.ModifyIssue;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Validators
{
	public class ModifyIssueDTOValidator : AbstractValidator<ModifyIssueCommand>
	{
		public ModifyIssueDTOValidator(IValidator<IssueItemDTO> itemValidator)
		{
			RuleFor(x => x.DTO.Id).NotEmpty()
				.WithMessage("Numer zamówienia wymagany");
			RuleFor(x => x.DTO.ClientId)
				.GreaterThan(0).WithMessage("Numer Klienta wymagany");
			RuleFor(x => x.DateToSend)
				.GreaterThan(DateTime.MinValue).WithMessage("Nie prawidłowa data zamówienia");
			RuleForEach(x => x.DTO.IssueItems).SetValidator(itemValidator);
			RuleFor(x => x.DTO.IssueItems)
				.NotEmpty().WithMessage("Brak ilości i/lub towaru w zamówieniu");
		}
	}
}