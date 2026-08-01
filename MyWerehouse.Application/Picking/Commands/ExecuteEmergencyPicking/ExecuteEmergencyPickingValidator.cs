using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Picking.Commands.ExecuteEmergencyPicking
{
	public class ExecuteEmergencyPickingValidator : AbstractValidator<ExecuteEmergencyPickingCommand>
	{
		public ExecuteEmergencyPickingValidator(ILocationRepo locationRepo)
		{
			RuleFor(p => p.RampNumber)
				.Cascade(CascadeMode.Stop)
				.GreaterThan(0)
				.WithMessage("Picking location must be specified.")
				.MustAsync(async (id, ct) => await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("The selected location does not exist.");
			RuleFor(p => p.PalletId)
				.NotEmpty()
				.WithMessage("Pallet ID must be specified.");
			RuleFor(p => p.IssueId)
				.NotEmpty()
				.WithMessage("Issue ID must be specified.");
			RuleFor(p => p.UserId)
				.NotEmpty()
				.WithMessage("User must be specified.");
		}
	}
}
