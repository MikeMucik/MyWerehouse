using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Common.ValueObject;

namespace MyWerehouse.Application.ViewModels.AddressModels
{
	public class AddAddressDTO : IMapFrom<Address>
	{
		public string Country { get; init; }
		public string City { get; init; }
		public string Region { get; init; }
		public int Phone { get; init; }
		public string PostalCode { get; init; }
		public string StreetName { get; init; }
		public string StreetNumber { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Address, AddAddressDTO>()
				.ReverseMap();
		}
	}
	public class AddAddressDTOValidation : AbstractValidator<AddAddressDTO>
	{
		public AddAddressDTOValidation()
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
