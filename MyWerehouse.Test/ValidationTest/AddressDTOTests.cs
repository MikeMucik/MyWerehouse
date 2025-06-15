using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.TestHelper;
using MyWerehouse.Application.ViewModels.AddressModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyWerehouse.Test.ValidationTest
{
	public class AddressDTOTests
	{
		[Fact]
		public void Add_Address_ProperDate_ShouldNotReturnValidationError()
		{
			var validator = new AddressDTOValidation();
			var address = new AddressDTO
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			validator.TestValidate(address).ShouldNotHaveAnyValidationErrors();
		}
		[Fact]
		public void Add_AddressInvalidCity_ShouldNotReturnValidationError()
		{
			var validator = new AddressDTOValidation();
			var address = new AddressDTO
			{
				//City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			validator.TestValidate(address).ShouldHaveValidationErrorFor(nameof(AddressDTO.City));
		}
		[Fact]
		public void Add_AddressInvalidCountry_ShouldNotReturnValidationError()
		{
			var validator = new AddressDTOValidation();
			var address = new AddressDTO
			{
				City = "Warsaw",
				//Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			validator.TestValidate(address).ShouldHaveValidationErrorFor(nameof(AddressDTO.Country));
		}
	}
}
