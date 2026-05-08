using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueDTO : IMapFrom<Issue>
	{
		public Guid Id { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		//public ClientDTO ClientData { get; set; } dopracować
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; }
		//public ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();	dopracować			
		public ICollection<PalletDTO> Pallets { get; set; } = new List<PalletDTO>();				
		public string PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public ICollection<IssueItemDTO> IssueItemsDTO { get; set; } = new List<IssueItemDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Issue, IssueDTO>();
		}
	}
}
