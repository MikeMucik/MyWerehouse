using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanCommandValidation : AbstractValidator<CreateReceiptPlanCommand>
	{
		public CreateReceiptPlanCommandValidation(IClientRepo clientRepo, ILocationRepo locationRepo)
		{
			RuleFor(x => x.DTO.ClientId)
				.MustAsync(async (id, ct) => await clientRepo.IsClientExistAsync(id))
				.WithMessage("The selected client does not exist.");
			RuleFor(x => x.DTO.ClientId)
				.GreaterThan(0).WithMessage("Client ID must be greater than zero.");
			RuleFor(l => l.DTO.RampNumber)
				.MustAsync(async (id, ct) => await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("The selected ramp does not exist.");
			RuleFor(x => x.DTO.PerformedBy)
				.NotEmpty().WithMessage("User is required.");
		}
	}
}
