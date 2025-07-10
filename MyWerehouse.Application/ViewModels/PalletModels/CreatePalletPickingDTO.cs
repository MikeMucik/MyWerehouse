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
		public PalletStatus Status { get; set; } = 0; 
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; set; } = new List<ProductOnPalletDTO>();
		public int IssueId { get; set; }
		public string? UserId { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<CreatePalletPickingDTO, Pallet>()
				.ForMember(dest => dest.ProductsOnPallet, opt => opt.MapFrom(src=>src.ProductsOnPallet));						
		}
	}
	public class CreatePalletPickingDTOValidation : AbstractValidator<CreatePalletPickingDTO>
	{
		public CreatePalletPickingDTOValidation(IValidator<ProductOnPalletDTO> productOnPalletValidator)
		{
			//RuleFor(p => p.Status)
			//	.NotEmpty()
			//	.WithMessage("Paleta musi mieć status");
			RuleFor(p => p.DateCreated)
				.NotEmpty()
				.WithMessage("Paleta musi mieć datę utworzenia");
			RuleFor(p => p.LocationId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć lokalizację początkową");
			RuleFor(p => p.IssueId)
				.GreaterThan(0)
				.WithMessage("Paleta musi mieć numer wydania");
			RuleFor(p => p.ProductsOnPallet)
				.NotEmpty()
				.WithMessage("Paleta musi zawierać towar/y");
			RuleFor(p => p.ProductsOnPallet)
				.Must(po => po.GroupBy(p => p.ProductId).All(g => g.Count() == 1))
				.WithMessage("Na palecie pickowanej każdy produkt może być tylko raz");
			RuleFor(p => p.ProductsOnPallet)
				.Must(po => po.GroupBy(pi=>pi.ProductId)
				.All(g=>g.Select(pp => pp.BestBefore)
				.Count() <= 1))
				.WithMessage("Każdy produkt musi mieć jedną datą BestBefore");
			RuleForEach(p => p.ProductsOnPallet)
				.SetValidator(productOnPalletValidator)
				.When(p => p.ProductsOnPallet != null && p.ProductsOnPallet.Count > 0);
		}
	}
}
