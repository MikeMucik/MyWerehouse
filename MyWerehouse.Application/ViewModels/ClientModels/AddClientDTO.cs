using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MyWerehouse.Application.ViewModels.AddressModels;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class AddClientDTO
	{
		public required string Name { get; set; }
		public required string Email { get; set; } 
		public required string Description { get; set; } 
		[MaxLength(250)]
		public required string FullName { get; set; } 
		public ICollection<AddressDTO> Addresses { get; set; } = new List<AddressDTO>();		
	}
	public class AddClientDTOValidation : AbstractValidator<AddClientDTO>
	{
		public AddClientDTOValidation(IValidator<AddressDTO> addressValidator)
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
