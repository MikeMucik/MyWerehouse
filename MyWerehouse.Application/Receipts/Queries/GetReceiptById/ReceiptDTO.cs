using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptById
{
	public class ReceiptDTO : IMapFrom<Receipt>
	{
			public Guid ReceiptId { get; init; }
			public int ReceiptNumber { get; init; }
			public int ClientId { get; init; }
			public string ClientName { get; set; } = string.Empty;
			public DateTime ReceiptDateTime { get; init; }
			public ICollection<PalletForReceiptViewDTO> Pallets { get; init; } = new List<PalletForReceiptViewDTO>();
			public string PerformedBy { get; init; } = string.Empty;
			public ReceiptStatus ReceiptStatus { get; init; }
			public int RampNumber { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Receipt, ReceiptDTO>()
				.ForMember(dest => dest.ReceiptId, opt => opt.MapFrom(static src => src.Id))
				.ForMember(dest=>dest.ClientName, opt=>opt.MapFrom(static src => src.Client.Name))
				.ForMember(dest => dest.Pallets, opt => opt.MapFrom(static src => src.Pallets));
		}
	}
}
