using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class IssueItemDTO
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }
	}
	public class IssueItemDTOValidion : AbstractValidator<IssueItemDTO>
	{
		public IssueItemDTOValidion()
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
