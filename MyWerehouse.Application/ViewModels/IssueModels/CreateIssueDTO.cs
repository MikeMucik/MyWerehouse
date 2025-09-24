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
	public class CreateIssueDTO : IMapFrom<Issue>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public DateTime IssueDateTime { get; set; } = DateTime.UtcNow;
		public string PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; } = IssueStatus.New;
		public List<IssueItemDTO> Items { get; set; } = new List<IssueItemDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreateIssueDTO, Issue>()
				.ForMember(d => d.Id, opt => opt.Ignore())
			.ForMember(d => d.IssueDateTimeCreate, opt => opt.Ignore())
			.ForMember(d => d.IssueStatus, opt => opt.Ignore());
			//.ForMember(d => d.PerformedBy, opt => opt.Ignore());//czy potrzebne te Ignore
		}
		public class CreateIssueDTOValidion : AbstractValidator<CreateIssueDTO>
		{
			public CreateIssueDTOValidion(IValidator<IssueItemDTO> itemValidator)
			{
				RuleFor(x => x.ClientId)
					.GreaterThan(0).WithMessage("Numer Klienta wymagany");
				RuleFor(x => x.IssueDateTime)
					.GreaterThan(DateTime.MinValue).WithMessage("Nie prawidłowa data zamówienia");
				RuleForEach(x => x.Items).SetValidator(itemValidator);
				RuleFor(x => x.Items)
					.NotEmpty().WithMessage("Brak palet z towarem w zamówieniu");
			}
		}
	}
}
