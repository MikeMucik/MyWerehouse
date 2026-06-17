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
		public string Name { get; init; }
		public string Email { get; init; }
		public string Description { get; init; }
		[MaxLength(250)]
		public string FullName { get; init; }
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
				.NotNull()
				.WithMessage("Uzupełnij dane - nazwa");
			RuleFor(c => c.Email)
				.NotNull()
				.WithMessage("Uzupełnij dane - email");
			RuleFor(c => c.FullName)
				.NotNull()
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
