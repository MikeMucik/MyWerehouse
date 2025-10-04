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
	public class UpdateIssueDTO : IMapFrom<Issue>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateToSend { get; set; }
		public List<IssueItemDTO> Items { get; set; } = new List<IssueItemDTO>();
		public class UpdateIssueDTOValidion : AbstractValidator<UpdateIssueDTO>
		{
			public UpdateIssueDTOValidion(IValidator<IssueItemDTO> itemValidator)
			{
				RuleFor(x => x.ClientId)
					.GreaterThan(0).WithMessage("Numer Klienta wymagany");
				RuleFor(x => x.DateToSend)
					.GreaterThan(DateTime.MinValue).WithMessage("Nie prawidłowa data zamówienia");
				RuleForEach(x => x.Items).SetValidator(itemValidator);
				RuleFor(x => x.Items)
					.NotEmpty().WithMessage("Brak ilości i/lub towaru w zamówieniu");
			}
		}
	}
}
