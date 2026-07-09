using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class UpdateClientDTO : IMapFrom<Client>
	{		
		public string Name { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string Description { get; init; } = string.Empty;
		[MaxLength(250)]
		public string FullName { get; init; } = string.Empty;
		public ICollection<EditAddressDTO> Addresses { get; init; } = new List<EditAddressDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<UpdateClientDTO, Client>()
				.ForMember(dest => dest.Addresses, opt => opt.Ignore());//robię przez synchronizera
		}
	}
	public class UpdateClientDTOValidation : AbstractValidator<UpdateClientDTO>
	{
		public UpdateClientDTOValidation(IValidator<EditAddressDTO> addressValidator)
		{
			RuleFor(c => c.Name)
				.NotEmpty()
				.WithMessage("Uzupełnij dane - nazwa");
			RuleFor(c => c.Email)
				.NotEmpty()
				.WithMessage("Uzupełnij dane - email");
			RuleFor(c => c.FullName)
				.NotEmpty()
				.WithMessage("Uzupełnij dane - pełna nazwa");
			RuleFor(c => c.Addresses)
				.NotEmpty()
				.WithMessage("Uzupełnij dane - adress");
			RuleForEach(c => c.Addresses)
				.SetValidator(addressValidator)
				.When(a => a.Addresses != null && a.Addresses.Count > 0);
		}
	}
}
