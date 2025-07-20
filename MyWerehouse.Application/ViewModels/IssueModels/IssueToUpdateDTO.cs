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
	public class IssueToUpdateDTO : IMapFrom<Issue>
	{
		public int Id { get; set; }
		public List<Pallet> pallets { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Issue, IssueItemDTO>();
		}
	}
}
