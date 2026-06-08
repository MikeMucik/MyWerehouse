using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Commands.CreateIssue
{
	public class CreateIssueCommandValidator : AbstractValidator<CreateIssueCommand>
	{
		public CreateIssueCommandValidator(IValidator<IssueItemDTO> itemValidator, IClientRepo clientRepo)
		{
			RuleFor(x => x.DTO.ClientId)
				.GreaterThan(0).WithMessage("Numer klienta wymagany");
			RuleFor(x => x.DTO.ClientId)
				.MustAsync(async (id, ct) => await clientRepo.IsClientExistAsync(id))
				.WithMessage("Wskazany klient nie istnieje.");
			RuleFor(x => x.DTO.PerformedBy)
				.NotEmpty().WithMessage("Użytkownik wymagany");
			RuleForEach(x => x.DTO.Items).SetValidator(itemValidator);
			RuleFor(x => x.DTO.Items)
				.NotEmpty().WithMessage("Brak ilości i/lub towaru w zamówieniu");
		}
	}
}