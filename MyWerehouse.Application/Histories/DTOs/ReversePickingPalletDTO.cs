using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class ReversePickingPalletDTO :IMapFrom<HistoryReversePicking>
	{
		public int Id { get; set; }
		public int ReversePickingId { get; set; }
		public string? PalletSourceId { get; set; }
		public string? PalletDestinationId { get; set; }
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus? StatusBefore { get; set; }
		public ReversePickingStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryReversePicking, ReversePickingPalletDTO>();
		}
	}
}
