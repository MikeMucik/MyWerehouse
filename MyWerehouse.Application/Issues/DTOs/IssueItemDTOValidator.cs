using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueItemDTOValidator : AbstractValidator<IssueItemDTO>
	{
		public IssueItemDTOValidator(IProductRepo productRepo, IDateTimeProvider dateTimeProvider)
		{
			RuleFor(x => x.ProductId)
				.NotEqual(Guid.Empty).WithMessage("Product ID is invalid.");
			RuleFor(x => x.ProductId)
				.MustAsync(async (id, ct) => await productRepo.IsExistProduct(id))
				.WithMessage("The selected product does not exist.");
			RuleFor(x => x.Quantity)
				.GreaterThan(0).WithMessage("Product quantity must be greater than zero.");
			RuleFor(x => x.BestBefore)
				.Must(date => date ==null || date > dateTimeProvider.Today)
				.WithMessage("Best-before date must be in the future.");
		}
	}
}
