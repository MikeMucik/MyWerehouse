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
	public class PalletHistoryDTO : IMapFrom<Pallet>
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public ICollection<PalletMovementDTO> PalletMovementsDTO { get; set; } = new List<PalletMovementDTO>();
		public int? ReceiptId { get; set; }
		public int? IssueId { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletHistoryDTO>()
				.ForMember(dest => dest.PalletMovementsDTO, opt => opt.Ignore());
		}
	}
}

