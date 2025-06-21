using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.TestHelper;
using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Application.ViewModels.ClientModels;

namespace MyWerehouse.Test.ValidationTest
{
	public class AddClientDTOTests
	{
		[Fact]
		public void AddClientProperData_ShouldNotReturnValidationError()
		{
			//Arrange
			var addressValidator = new AddressDTOValidation();
			var validator = new AddClientDTOValidation(addressValidator);
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
			var client = new AddClientDTO
			{
				Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<AddressDTO> { address },
				Description = "description",
			};
			//Act&Assert
			validator.TestValidate(client).ShouldNotHaveAnyValidationErrors();
		}
		[Fact]
		public void AddClientNotProperData_ShouldNotReturnValidationError()
		{
			//Arrange
			var addressValidator = new AddressDTOValidation();
			var validator = new AddClientDTOValidation(addressValidator);
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
			var client = new AddClientDTO
			{
				Name = "name",
				//FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<AddressDTO> { address },
				Description = "description",
			};
			//Act&Assert
			validator.TestValidate(client).ShouldHaveValidationErrorFor(nameof(AddClientDTO.FullName));
		}
		[Fact]
		public void AddClientNoAddressData_ShouldNotReturnValidationError()
		{
			//Arrange
			var addressValidator = new AddressDTOValidation();
			var validator = new AddClientDTOValidation(addressValidator);
			var client = new AddClientDTO
			{
				Name = "name",
				FullName = "fullname",
				Email = "email@wp.pl",
				Addresses = new List<AddressDTO> { },
				Description = "description",
			};
			//Act&Assert
			validator.TestValidate(client).ShouldHaveValidationErrorFor(nameof(AddClientDTO.Addresses));
		}		
	}
}
