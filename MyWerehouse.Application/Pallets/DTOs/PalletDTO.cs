using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.Histories.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class PalletDTO : IMapFrom<Pallet>//TODO Search
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }		
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new HashSet<ProductOnPalletDTO>();
		public ICollection<PalletMovementDTO> PalletMovements { get; set; } = new List<PalletMovementDTO>();
		public Guid? ReceiptId { get; set; }		
		public int? ReceiptNumber { get; set; }		//
		public Guid? IssueId { get; set; }
		public int? IssueNumber { get; set; }//
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Pallet, PalletDTO>();
			//profile.CreateMap<PalletDTO, Pallet>()
				//.ForMember(dest => dest.PalletMovements, opt => opt.Ignore());
		}
		
	}
}
//public class PalletDTOValidation : AbstractValidator<PalletDTO>
		//{
		//	public PalletDTOValidation() 
		//	{
		//		RuleFor(p => p.ProductsOnPallet)
		//			.NotNull()
		//			.WithMessage("Paleta musi zawierać produkty");
		//	}
		//}