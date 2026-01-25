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

namespace MyWerehouse.Application.Receipts.DTOs
{
	public class CreatePalletReceiptDTO :IMapFrom<Pallet>
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }		
		public PalletStatus Status { get; set; } = 0; 
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();	
		public int ReceiptId { get; set; }		
		public string UserId { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreatePalletReceiptDTO, Pallet>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(src => src.ProductsOnPallet));				
		}
	}
	//public class CreatePalletReceiptDTOValidation : AbstractValidator<CreatePalletReceiptDTO> 
	//{
	//	public CreatePalletReceiptDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
	//	{
	//		RuleFor(p => p.Status)
	//			.NotEmpty()
	//			.WithMessage("Paleta musi mieć status")
	//			.When(p=> !string.IsNullOrWhiteSpace( p.Id));
	//		RuleFor(p => p.DateReceived)
	//			.NotEmpty()
	//			.WithMessage("Paleta musi mieć datę przyjęcia")
	//			.When(p => !string.IsNullOrWhiteSpace(p.Id));
	//		RuleFor(p => p.LocationId)
	//			.GreaterThan(0)
	//			.WithMessage("Paleta musi mieć lokalizację początkową")
	//			.When(p => !string.IsNullOrWhiteSpace(p.Id));
	//		RuleFor(p => p.ReceiptId)
	//			.GreaterThan(0)
	//			.WithMessage("Paleta musi mieć numer przyjęcia")
	//			.When(p => !string.IsNullOrWhiteSpace(p.Id));
	//		RuleFor(p => p.ProductsOnPallet)
	//			.NotEmpty()
	//			.WithMessage("Paleta musi zawierać towar/y");
	//		RuleFor(p => p.ProductsOnPallet)
	//			.Must(po => po.Select(p => p.ProductId)
	//			.Distinct().Count() <= 1)
	//			.WithMessage("Paleta przyjmowana może mieć tylko jeden rodzaj produktu");
	//		RuleFor(p => p.ProductsOnPallet)
	//			.Must(po => po.Select(po => po.BestBefore)
	//			.Distinct()
	//			.Count() <= 1)
	//			.WithMessage("Produkt musi mieć jedną datą BestBefore");				;
	//		RuleForEach(p => p.ProductsOnPallet)
	//			.SetValidator(productOnPalletValidator)
	//			.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
	//	}
	//}
}
