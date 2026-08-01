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
		public required string Country { get; init; } 
		public required string City { get; init; } 
		public required string Region { get; init; } 
		public int Phone { get; init; }
		public required string PostalCode { get; init; } 
		public required string StreetName { get; init; } 
		public required string StreetNumber { get; init; } 
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
				.NotEmpty()
				.WithMessage("City is required.");
			RuleFor(a => a.Region)
				.NotEmpty()
				.WithMessage("Region is required.");
			RuleFor(a => a.PostalCode)
				.NotEmpty()
				.WithMessage("Postal code is required.");
			RuleFor(a => a.StreetName)
				.NotEmpty()
				.WithMessage("Street name is required.");
			RuleFor(a => a.StreetNumber)
				.NotEmpty()
				.WithMessage("Street number is required.");
			RuleFor(a => a.Country)
				.NotEmpty()
				.WithMessage("Country is required.");
			RuleFor(a => a.Phone)
				.NotNull()
				.NotEqual(0)
				.WithMessage("Phone number is required.");
		}
	}
}
