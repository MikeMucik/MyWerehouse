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
	public class CreatePalletPickingDTO : IMapFrom<Pallet>
	{
		public string Id { get; set; }
		public DateTime DateCreated { get; set; }
		public int LocationId { get; set; }
		public PalletStatus Status { get; set; } = 0; //np "Available", "To issue"
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();
		public int IssueId { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreatePalletPickingDTO, Pallet>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.Ignore());			
		}
	}
	public class CreatePalletPickingDTOValidation : AbstractValidator<CreatePalletPickingDTO>
	{
		public CreatePalletPickingDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			RuleFor(p => p.Status)
				.NotNull()
				.WithMessage("Paleta musi mieć status");
			RuleFor(p => p.DateCreated)
				.NotNull()
				.WithMessage("Paleta musi mieć datę utworzenia");
			RuleFor(p => p.LocationId)
				.NotNull()
				.WithMessage("Paleta musi mieć lokalizację początkową");
			RuleFor(p => p.IssueId)
				.NotNull()
				.WithMessage("Paleta musi mieć numer wydania");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count() > 0);
		}
	}
}
