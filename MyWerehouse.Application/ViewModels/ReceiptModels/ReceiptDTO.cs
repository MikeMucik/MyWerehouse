using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.ReceiptModels
{
	public class ReceiptDTO : IMapFrom<Receipt>
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public ICollection<UpdatePalletDTO> Pallets { get; set; } = new List<UpdatePalletDTO>();
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<ReceiptDTO, Receipt>()
				//.ForMember(dest => dest.Pallets, opt => opt.UseDestinationValue());
				.ForMember(dest => dest.Pallets, opt => opt.Ignore());

			profile.CreateMap<ReceiptDTO, Receipt>()
				.ForMember(dest => dest.Pallets, opt => opt.MapFrom(src => src.Pallets)); 
		}
	}
	public class ReceiptDTOValidation : AbstractValidator<ReceiptDTO>
	{
		public ReceiptDTOValidation(IValidator<UpdatePalletDTO> palletValidator)
		{
			RuleFor(r => r.Id)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć swój numer.");
			RuleFor(r => r.ClientId)
				.GreaterThan(0)
				.WithMessage("Przyjęcie musi mieć numer klienta");
			RuleFor(r => r.ReceiptDateTime)
				.NotEqual(default(DateTime))
				.WithMessage("Przyjęcie musi mieć datę.");
			RuleFor(r => r.Pallets)
				.NotEmpty()
				.WithMessage("Przyjęcie musi zawierać przyjęte palety");
			//RuleFor(r => r.PerformedBy)
			RuleForEach(p => p.Pallets)
				.SetValidator(palletValidator)
				.When(p => p.Pallets != null && p.Pallets.Any());
		}
	}
}
