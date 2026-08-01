using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public class ModifyIssueDTOValidator : AbstractValidator<ModifyIssueCommand>
	{
		public ModifyIssueDTOValidator(IValidator<IssueItemDTO> itemValidator, IClientRepo clientRepo)
		{
			RuleFor(x => x.Id).NotEmpty()
				.WithMessage("Issue ID is required.");
			RuleFor(x => x.DTO.ClientId)
				.GreaterThan(0).WithMessage("Client ID must be greater than zero.");
			RuleFor(x => x.DTO.ClientId)
				.MustAsync(async (id,ct) => await clientRepo.IsClientExistAsync(id))
				.WithMessage("The selected client does not exist.");
			RuleFor(x => x.DTO.PerformedBy)
				.NotEmpty().WithMessage("User is required.");
			RuleFor(x => x.DateToSend)
				.GreaterThan(DateOnly.FromDateTime(DateTime.MinValue)).WithMessage("Issue date is invalid.");
			RuleForEach(x => x.DTO.IssueItems).SetValidator(itemValidator);
			RuleFor(x => x.DTO.IssueItems)
				.NotEmpty().WithMessage("An issue must contain at least one product.");
		}
	}
}
