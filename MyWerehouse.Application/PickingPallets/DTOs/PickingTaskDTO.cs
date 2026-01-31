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
		public int Id { get; set; }
		public int IssueId { get; set; }
		public required string SourcePalletId { get; set; }		
		public int ProductId { get; set; }
		public int RequestedQuantity { get; set; }
		public int PickedQuantity { get; set; }//faktyczna pobrana ilość
		public PickingStatus PickingStatus { get; set; }
		public DateOnly? BestBefore { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PickingTask, PickingTaskDTO>()
				.ReverseMap();
		}
	}
}
