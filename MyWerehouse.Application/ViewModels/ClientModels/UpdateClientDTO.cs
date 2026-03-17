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
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		[MaxLength(250)]
		public string FullName { get; set; }
		public ICollection<AddressDTO> Addresses { get; set; } = new List<AddressDTO>();
		public void Mapping(Profile profile)
		{
			profile.CreateMap<UpdateClientDTO, Client >()
				.ForMember(dest => dest.Addresses, opt => opt.Ignore());
		}
	}
	public class UpdateClientDTOValidation : AbstractValidator<UpdateClientDTO>
	{
		public UpdateClientDTOValidation(IValidator<AddressDTO> addressValidator)
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
