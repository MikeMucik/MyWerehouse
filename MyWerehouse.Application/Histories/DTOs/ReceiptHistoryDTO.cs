using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public	class ReceiptHistoryDTO : IMapFrom<HistoryReceipt>
	{
		public int Id { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public int CLientId { get; set; }
		public ReceiptStatus ReceiptStatus { get; set; }
		public string PerformedBy { get; set; }
		public ICollection<PalletListDTO> ListDTOs { get; set; } = new List<PalletListDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<HistoryReceipt, ReceiptHistoryDTO>()
				.ForMember(dest => dest.ListDTOs, opt => opt.Ignore());
		}
	}
}
