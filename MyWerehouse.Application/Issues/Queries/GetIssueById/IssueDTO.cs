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
	public class IssueDTO 
	{
		public Guid Id { get; init; }
		public int IssueNumber { get; init; }
		public int ClientId { get; init; }
		public string ClientName { get; init; } = string.Empty;
		public DateTime IssueDateTimeCreate { get; init; }
		public DateOnly IssueDateTimeSend { get; init; }		
		public ICollection<PalletDTOIssue> Pallets { get; init; } = new List<PalletDTOIssue>();
		public string PerformedBy { get; init; } = string.Empty;
		public IssueStatus IssueStatus { get; init; }
		public ICollection<IssueItemViewDTO> IssueItems { get; init; } = new List<IssueItemViewDTO>();		
	}
}
