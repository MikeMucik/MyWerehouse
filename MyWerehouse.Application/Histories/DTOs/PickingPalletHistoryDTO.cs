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
	public class PickingPalletHistoryDTO : IMapFrom<HistoryPicking>
	{
		public int Id { get; set; }
		public int? PickingTaskId { get; set; }									  //[JsonIgnore] // Ignoruj przy serializacji
		public string PalletId { get; set; }
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int QuantityAllocated { get; set; }   
		public int QuantityPicked { get; set; }      
		public PickingStatus StatusBefore { get; set; }
		public PickingStatus StatusAfter { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateTime { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryPicking, PickingPalletHistoryDTO>();
		}
	}
}
