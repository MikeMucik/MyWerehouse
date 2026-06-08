using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class UpdateReceiptDTO : IMapFrom<Receipt>
	{		
		public int ClientId { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public ICollection<EditPalletInReceiptDTO> Pallets { get; set; } = new List<EditPalletInReceiptDTO>();
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public int RampNumber { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Receipt, ReceiptDTO>()
				.ForMember(dest => dest.ReceiptId, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Pallets, opt => opt.MapFrom(src => src.Pallets));			
		}
	}
}