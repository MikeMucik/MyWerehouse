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
	public class UpdatePalletDTO : IMapFrom<Pallet>
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();
		public int? ReceiptId { get; set; }
		public int? IssueId { get; set; }
		public string? UserId { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<UpdatePalletDTO, Pallet>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.Ignore())
				//.ForMember(dest => dest.Id, opt=>opt.Ignore())
				;
			profile.CreateMap<Pallet, UpdatePalletDTO>()
				.ForMember(dest => dest.ProductsOnPallet, opt=>opt.MapFrom(src=>src.ProductsOnPallet));
		}
	}
	public class UpdatePalletDTOValidation : AbstractValidator<UpdatePalletDTO>
	{
		public UpdatePalletDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
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
