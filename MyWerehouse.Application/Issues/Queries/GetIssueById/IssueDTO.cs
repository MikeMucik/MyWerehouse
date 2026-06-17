using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class IssueDTO : IMapFrom<Issue>
	{
		public Guid Id { get; init; }
		public int IssueNumber { get; init; }
		public int ClientId { get; init; }
		//public ClientDTO ClientData { get; init; } dopracować ?
		public DateTime IssueDateTimeCreate { get; init; }
		public DateOnly IssueDateTimeSend { get; init; }		
		public ICollection<PalletDTOIssue> Pallets { get; init; } = new List<PalletDTOIssue>();
		public string PerformedBy { get; init; }
		public IssueStatus IssueStatus { get; init; }
		public ICollection<IssueItemDTO> IssueItemsDTO { get; init; } = new List<IssueItemDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Issue, IssueDTO>()
				.ForMember(dest => dest.IssueItemsDTO, opt => opt.MapFrom(src => src.IssueItems));
		}
	}
}