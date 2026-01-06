using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueDTO : IMapFrom<Issue>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; }
		public ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();				
		public string PerformedBy { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public ICollection<IssueItem> IssueItems { get; set; } = new List<IssueItem>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Issue, IssueDTO>();
		}
	}
}
