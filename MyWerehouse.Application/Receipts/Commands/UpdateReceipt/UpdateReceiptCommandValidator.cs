using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Queries.GetReceiptById;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Persistence.Repositories;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptCommandValidator : AbstractValidator<UpdateReceiptCommand>
	{
		public UpdateReceiptCommandValidator(IValidator<EditPalletInReceiptDTO> palletValidator, IClientRepo clientRepo, ILocationRepo locationRepo)
		{
			RuleFor(r => r.Id)
				.NotEqual(Guid.Empty)
				.WithMessage("Receipt ID is required.");
			RuleFor(x => x.DTO.ClientId)
				.MustAsync(async (id, ct) => await clientRepo.IsClientExistAsync(id))
				.WithMessage("The selected client does not exist.");
			RuleFor(r => r.DTO.ClientId)
				.GreaterThan(0)
				.WithMessage("Client ID must be greater than zero.");
			RuleFor(p => p.DTO.RampNumber)
				.GreaterThan(0)
				.WithMessage("Receipt ramp is required.");
			RuleFor(l => l.DTO.RampNumber)
				.MustAsync(async (id, ct) => await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("The selected ramp does not exist.");
			RuleFor(r => r.DTO.Pallets)
				.NotEmpty()
				.WithMessage("Receipt must contain at least one pallet.");
			RuleFor(r => r.DTO.PerformedBy)
				.NotEmpty()
				.WithMessage("User is required.");
			RuleForEach(p => p.DTO.Pallets)
				.SetValidator(palletValidator)
				.When(p => p.DTO.Pallets != null && p.DTO.Pallets.Any());
		}
	}
}
