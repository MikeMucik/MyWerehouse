using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.Queries.GetReceiptById
{
	public class ReceiptDTO : IMapFrom<Receipt>
	{
		public Guid ReceiptId { get; init; }
		public int ReceiptNumber { get; init; }
		public int ClientId { get; init; }
		public DateTime ReceiptDateTime { get; init; }
		public ICollection<EditPalletInReceiptDTO> Pallets { get; init; } = new List<EditPalletInReceiptDTO>();
		public string PerformedBy { get; init; } 
		public ReceiptStatus ReceiptStatus { get; init; }
		public int RampNumber { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Receipt, ReceiptDTO>()
				.ForMember(dest => dest.ReceiptId, opt => opt.MapFrom(src => src.Id))
				.ForMember(dest => dest.Pallets, opt => opt.MapFrom(src => src.Pallets));
		}
	}
}