using MyWerehouse.Application.ViewModels.AddressModels;
using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Application.ViewModels.ClientModels
{
	public class ClientDTO
	{
		public int Id { get; init; }
		public string Name { get; init; } = string.Empty;
		public string Email { get; init; } = string.Empty;
		public string Description { get; init; } = string.Empty;
		public string FullName { get; init; } = string.Empty;
		public ICollection<AddressDTO> Addresses { get; init; } = new List<AddressDTO>();	
	}	
}
