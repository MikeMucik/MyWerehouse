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
		public string? SourcePalletId { get; set; }      //required 
		public int ProductId { get; set; }
		public int RequestedQuantity { get; set; }
		public int PickedQuantity { get; set; }//faktyczna pobrana ilość
		public PickingStatus PickingStatus { get; set; }
		public DateOnly? BestBefore { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PickingTask, PickingTaskDTO>()
				
				;
			//.ForMember(d => d.Id, opt => opt.MapFrom(s => s.PickingTaskNumber));
			profile.CreateMap<PickingTaskDTO, PickingTask>()
				//.ForMember(d=>d.I)
				;
				//.ForMember(d => d.IssueNumber, opt => opt.MapFrom(s => s.Id));
				//.ForMember(d=>d.)

		}
	}
}
