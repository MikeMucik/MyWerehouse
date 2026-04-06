using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.DTOs
{
	public class PickingTaskDTO : IMapFrom<PickingTask>
	{
		public Guid Id { get; set; }
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public Guid? SourcePalletId { get; set; }      //required 
		public string? SourcePalletNumber { get; set; }      //required 
		public Guid ProductId { get; set; }
		public int RequestedQuantity { get; set; }
		public int PickedQuantity { get; set; }//faktyczna pobrana ilość
		public PickingStatus PickingStatus { get; set; }
		public DateOnly? BestBefore { get; set; }

		public int RampNumber { get; set; } //lokalizacja pickingu
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PickingTask, PickingTaskDTO>()
				.ForMember(dest => dest.IssueNumber, opt => opt.MapFrom(src => src.Issue.IssueNumber));			
		}
	}
}
