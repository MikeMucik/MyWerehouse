using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class IssueHistoryDTO : IMapFrom<HistoryIssue>
	{
		public int Id { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; }
		public int CLientId { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public string PerformedBy { get; set; }
		public ICollection<PalletListDTO> ListDTOs { get; set; } = new List<PalletListDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryIssue, IssueHistoryDTO>()
				.ForMember(dest => dest.ListDTOs, opt => opt.Ignore());
		}
	}
}
