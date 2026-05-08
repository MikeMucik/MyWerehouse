using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Picking.DTOs;

namespace MyWerehouse.Application.Picking.Validators
{
	public class PickingTaskDTOValidator : AbstractValidator<PickingTaskDTO>
	{
		public PickingTaskDTOValidator()
		{
			RuleFor(p => p.RampNumber)
				.NotEmpty()
				.WithMessage("Określ miejsce kompletacji");
		}
	}
}
