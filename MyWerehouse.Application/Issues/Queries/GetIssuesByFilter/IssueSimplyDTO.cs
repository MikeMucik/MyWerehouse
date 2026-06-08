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
		public Guid Id { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; }
		public string PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Issue, IssueSimplyDTO>();
		}
	}
}