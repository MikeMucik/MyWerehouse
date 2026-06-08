using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueItemDTOValidator : AbstractValidator<IssueItemDTO>
	{
		public IssueItemDTOValidator(IProductRepo productRepo)
		{
			RuleFor(x => x.ProductId)
				.NotEqual(Guid.Empty).WithMessage("Nieprawidłowy numer produktu");
			RuleFor(x => x.ProductId)
				.MustAsync(async (id, ct) => await productRepo.IsExistProduct(id))
				.WithMessage("Produkt nie istnieje.");
			RuleFor(x => x.Quantity)
				.GreaterThan(0).WithMessage("Ilość produktu musi być większa dod zera");
			RuleFor(x => x.BestBefore)
				.Must(date => date > DateOnly.FromDateTime(DateTime.Now))
				.WithMessage("Data do spożycia musi być datą z przyszłości");
		}
	}
}
