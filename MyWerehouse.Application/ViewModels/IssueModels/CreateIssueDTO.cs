using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class CreateIssueDTO : IMapFrom<Issue>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public DateTime IssueDateTime { get; set; }
		public string? PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public List<IssueItemDTO> Items { get; set; } = new List<IssueItemDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreateIssueDTO, Issue>()
				.ForMember(d => d.Id, opt => opt.Ignore())
			.ForMember(d =>d.IssueDateTimeCreate, opt => opt.Ignore())
			.ForMember(d => d.IssueStatus, opt => opt.Ignore())
			.ForMember(d => d.PerformedBy, opt => opt.Ignore());//czy potrzebne te Ignore
		}		
	}
}
