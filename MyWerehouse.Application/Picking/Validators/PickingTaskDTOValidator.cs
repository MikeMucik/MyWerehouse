using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Picking.Validators
{
	public class PickingTaskDTOValidator : AbstractValidator<PickingTaskDTO>
	{
		public PickingTaskDTOValidator(ILocationRepo locationRepo)
		{
			RuleFor(p => p.RampNumber)
				.NotEmpty()
				.WithMessage("Określ miejsce kompletacji");
			RuleFor(p => p.RampNumber)
				.MustAsync(async (id,ct)=> await locationRepo.ReceivingRampExistsAsync(id))
				.WithMessage("Wybrane lokalizacja nie istnieje.");
		}
	}
}
