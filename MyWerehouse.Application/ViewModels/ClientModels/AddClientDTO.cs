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
	public class AddClientDTO :IMapFrom<Client>
	{
		public required string Name { get; set; }
		public required string Email { get; set; } 
		public required string Description { get; set; } 
		[MaxLength(250)]
		public required string FullName { get; set; } 
		public ICollection<AddAddressDTO> Addresses { get; set; } = new List<AddAddressDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<AddClientDTO, Client>();
		}
	}
	public class AddClientDTOValidation : AbstractValidator<AddClientDTO>
	{
		public AddClientDTOValidation(IValidator<AddAddressDTO> addressValidator)
		{			
			RuleFor(c => c.Name)
				.NotEmpty()
				.WithMessage("Client name is required.");
			RuleFor(c => c.Email)
				.NotEmpty()
				.WithMessage("Client email is required.");
			RuleFor(c => c.FullName)
				.NotEmpty()
				.WithMessage("Client full name is required.");
			RuleFor(c => c.Addresses)
				.NotEmpty()
				.WithMessage("At least one client address is required.");
			RuleForEach(c => c.Addresses)
				.SetValidator(addressValidator)
				.When(a => a.Addresses != null && a.Addresses.Count > 0);
		}
	}
}
