using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.DTOs
{
	public class ReceiptSimplyDTO : IMapFrom<Receipt>
	{
		public Guid ReceiptId { get; set; }
		public int ReceiptNumber { get; set; }
		public int ClientId { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public int RampNumber { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Receipt, ReceiptSimplyDTO>()
				.ForMember(dest => dest.ReceiptId, opt => opt.MapFrom(src => src.Id));
		}
	}
}
