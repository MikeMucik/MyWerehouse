using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.AddressModels
{
	public class AddressDTO : IMapFrom<Address>
	{
		public int Id { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public int Phone { get; set; }
		public string PostalCode { get; set; }
		public string StreetName { get; set; }
		public string StreetNumber { get; set; }
		//public string AdditionalEmail { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Address, AddressDTO>()
				.ReverseMap();
		}
	}
	public class AddressDTOValidation : AbstractValidator<AddressDTO>
	{
		public AddressDTOValidation()
		{
			RuleFor(a => a.City)
				.NotNull()
				.WithMessage("Uzupełnij dane - miasto");
			RuleFor(a => a.Region)
				.NotNull()
				.WithMessage("Uzupełnij dane - województwo");
			RuleFor(a => a.PostalCode)
				.NotNull()
				.WithMessage("Uzupełnij dane - numer pocztowy");
			RuleFor(a => a.StreetName)
				.NotNull()
				.WithMessage("Uzupełnij dane - nazwa ulicy");
			RuleFor(a => a.StreetNumber)
				.NotNull()
				.WithMessage("Uzupełnij dane - numer domu/lokalu");
			RuleFor(a => a.Country)
				.NotNull()
				.WithMessage("Uzupełnij dane - nazwa państwa");
			RuleFor(a => a.Phone)
				.NotNull()
				.NotEqual(0)
				.WithMessage("Uzupełnij dane - numer telefonu");
		}
	}
}
