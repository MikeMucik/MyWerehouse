using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class PalletDTO : IMapFrom<Pallet>
	{
		public Guid Id { get; set; }
		public string PalletNumber { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new HashSet<ProductOnPalletDTO>();
		public ICollection<HistoryPalletDTO> PalletMovements { get; set; } = new List<HistoryPalletDTO>();
		public Guid? ReceiptId { get; set; }
		public int? ReceiptNumber { get; set; }     //
		public Guid? IssueId { get; set; }
		public int? IssueNumber { get; set; }//
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletDTO>();
		}
	}
}
