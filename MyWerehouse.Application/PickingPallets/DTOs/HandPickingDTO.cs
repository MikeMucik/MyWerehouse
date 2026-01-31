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
	public class HandPickingDTO : IMapFrom<HandPickingTask>
	{
		public int Id { get; set; }
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public PickingStatus PickingStatus { get; set; }
		public DateOnly? BestBefore { get; set; }
		public int PickedQuantity { get; set; }
		public DateTime CreateDate { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HandPickingDTO, HandPickingTask>()
				.ReverseMap();
		}
	}
}
