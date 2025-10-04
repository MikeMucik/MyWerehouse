using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public record IssueItemDTO :IMapFrom<IssueItem>
	{
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<IssueItemDTO, IssueItem>();
		}
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
