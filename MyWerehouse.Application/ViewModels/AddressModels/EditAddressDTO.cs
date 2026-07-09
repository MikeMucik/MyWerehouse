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
	public class EditAddressDTO : IMapFrom<Address>
	{
		public int Id { get; init; }
		public required string Country { get; init; } 
		public required string City { get; init; } 
		public required string Region { get; init; } 
		public int Phone { get; init; }
		public required string PostalCode { get; init; } 
		public required string StreetName { get; init; } 
		public required string StreetNumber { get; init; } 
		//public string AdditionalEmail { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Address, EditAddressDTO>()
				.ReverseMap();
		}
	}
	public class EditAddressDTOValidation : AbstractValidator<EditAddressDTO>
	{
		public EditAddressDTOValidation()
		{
			RuleFor(a => a.City)
				.NotNull()
				.NotEmpty()
				.WithMessage("Uzupełnij dane - miasto");
			RuleFor(a => a.Region)
				.NotNull()
				.NotEmpty()
				.WithMessage("Uzupełnij dane - województwo");
			RuleFor(a => a.PostalCode)
				.NotNull()
				.NotEmpty()
				.WithMessage("Uzupełnij dane - numer pocztowy");
			RuleFor(a => a.StreetName)
				.NotNull()
				.NotEmpty()
				.WithMessage("Uzupełnij dane - nazwa ulicy");
			RuleFor(a => a.StreetNumber)
				.NotNull()
				.NotEmpty()
				.WithMessage("Uzupełnij dane - numer domu/lokalu");
			RuleFor(a => a.Country)
				.NotNull()
				.NotEmpty()
				.WithMessage("Uzupełnij dane - nazwa państwa");
			RuleFor(a => a.Phone)
				.NotNull()
				.NotEqual(0)
				.WithMessage("Uzupełnij dane - numer telefonu");
		}
	}
}
