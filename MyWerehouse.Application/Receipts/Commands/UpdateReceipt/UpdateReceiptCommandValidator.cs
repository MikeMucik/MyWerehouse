using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Pallets.DTOs;
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
				.WithMessage("Przyjęcie musi mieć swój numer.");
			RuleFor(x => x.DTO.ClientId)
				.MustAsync(async (id, ct) => await clientRepo.IsClientExistAsync(id))
				.WithMessage("Wybrany klient nie istnieje.");
			RuleFor(r => r.DTO.ClientId)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć numer klienta.");
			RuleFor(p => p.DTO.RampNumber)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację.");
			RuleFor(l => l.DTO.RampNumber)
				.MustAsync(async (id, ct) => await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("Wybrana rampa nie istnieje.");
			RuleFor(r => r.DTO.Pallets)
				.NotEmpty()
				.WithMessage("Przyjęcie musi zawierać przyjęte palety.");
			RuleFor(r => r.DTO.PerformedBy)
				.NotEmpty()
				.WithMessage("Przyjecie musi zawierać użytkownika.");
			RuleForEach(p => p.DTO.Pallets)
				.SetValidator(palletValidator)
				.When(p => p.DTO.Pallets != null && p.DTO.Pallets.Any());
		}
	}
}
