using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Validators
{
	public class IssueItemDTOValidation : AbstractValidator<IssueItemDTO>
	{
		public IssueItemDTOValidation()
		{
			RuleFor(x => x.ProductId)
				.GreaterThan(0).WithMessage("Nieprawidłowy numer produktu");
			RuleFor(x => x.Quantity)
				.GreaterThan(0).WithMessage("Ilość produktu musi być większa dod zera");
			RuleFor(x => x.BestBefore)
				.Must(date => date > DateOnly.FromDateTime(DateTime.Now))
				.WithMessage("Data do spożycia musi być z datą z przyszłości");
		}
	}
}
