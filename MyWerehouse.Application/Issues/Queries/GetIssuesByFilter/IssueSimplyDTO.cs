using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssuesByFilter
{
	public class IssueSimplyDTO : IMapFrom<Issue>
	{
		public Guid Id { get; init; }
		public int IssueNumber { get; init; }
		public int ClientId { get; init; }
		public DateTime IssueDateTimeCreate { get; init; }
		public DateOnly IssueDateTimeSend { get; init; }
		public string PerformedBy { get; init; }
		public IssueStatus IssueStatus { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Issue, IssueSimplyDTO>()
				.ForMember(dest => dest.IssueDateTimeCreate, opt=>opt.MapFrom(src=>src.IssueDateTimeCreate))
				.ForMember(dest => dest.IssueDateTimeSend, opt=>opt.MapFrom(src=>src.IssueDateTimeSend));
		}
	}
}