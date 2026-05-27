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
	public class EditPalletInReceiptDTOValidator : AbstractValidator<EditPalletInReceiptDTO>
	{
		public EditPalletInReceiptDTOValidator(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			//
			RuleFor(p => p.Status)
				.NotEmpty()
				.WithMessage("Paleta musi mieć status");
			RuleFor(p => p.DateReceived)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę utworzenia");
			RuleFor(p => p.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
		}
	}
}
