using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Receipts.Commands.UpdateReceipt
{
	public class EditPalletInReceiptDTO :IMapFrom<Pallet>
	{
		public Guid Id { get; set; }
		public string PalletNumber { get; set; }//
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public PalletStatus Status { get; set; } = 0;
		public string UserId { get; set; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();

		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, EditPalletInReceiptDTO>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(src => src.ProductsOnPallet));
		}
	}	
}
