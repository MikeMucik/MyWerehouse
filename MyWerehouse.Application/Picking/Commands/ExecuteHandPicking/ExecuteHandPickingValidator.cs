using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Picking.Commands.ExecuteHandPicking
{
	public class ExecuteHandPickingValidator : AbstractValidator<ExecuteHandPickingCommand>
	{
		public ExecuteHandPickingValidator(ILocationRepo locationRepo)
		{
			RuleFor(p => p.RampNumber)
				.Cascade(CascadeMode.Stop)
				.GreaterThan(0)
				.WithMessage("Picking location must be specified.")
				.MustAsync(async (id, ct) => await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("The selected location does not exist.");
			RuleFor(p => p.PalletIdSource)
				.NotEmpty()
				.WithMessage("Source pallet ID must be specified.");
			RuleFor(p => p.IssueId)
				.NotEmpty()
				.WithMessage("Issue ID must be specified.");
			RuleFor(p => p.PickedQuantity)
				.GreaterThan(0)
				.WithMessage("Picked quantity must be greater than zero.");
			RuleFor(p => p.UserId)
				.NotEmpty()
				.WithMessage("User must be specified.");
		}
	}
}
