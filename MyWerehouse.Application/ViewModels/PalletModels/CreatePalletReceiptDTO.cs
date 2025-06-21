using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.PalletModels
{
	public class CreatePalletReceiptDTO :IMapFrom<Pallet>
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }		
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();	
		public int ReceiptId { get; set; }		
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreatePalletReceiptDTO, Pallet>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.Ignore());
				//.MapFrom(src => src.ProductsOnPallet)); 
		}
	}
	public class CreatePalletReceiptDTOValidation : AbstractValidator<CreatePalletReceiptDTO> 
	{
		public CreatePalletReceiptDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.Status)
				.NotNull()
				.WithMessage("Paleta musi mieć status");
			RuleFor(p => p.DateReceived)
				.NotNull()
				.WithMessage("Paleta musi mieć datę przyjęcia");
			RuleFor(p => p.LocationId)
				.NotNull()
				.WithMessage("Paleta musi mieć lokalizację początkową");
			RuleFor(p => p.ReceiptId)
				.NotNull()
				.WithMessage("Paleta musi mieć numer przyjęcia");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count() > 0);
		}
	}
}

//public ICollection<PalletMovement> PalletMovements { get; set; } = new List<PalletMovement>();