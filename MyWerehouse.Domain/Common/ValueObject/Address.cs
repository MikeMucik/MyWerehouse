using MyWerehouse.Domain.Clients.Models;

namespace MyWerehouse.Domain.Common.ValueObject
{
	public class Address
	{
		public int Id { get; set; }			
		public string Country { get; set; } = string.Empty;
		public string City { get; set; } = string.Empty;
		public string Region { get; set; } = string.Empty;
		public int Phone { get; set; }
		public string PostalCode { get; set; } = string.Empty;
		public string StreetName { get; set; } = string.Empty;
		public string StreetNumber { get; set; } = string.Empty;
		public string? AdditionalEmail { get; set; } 
		public int ClientId { get; set; }
		public Client Clients { get; set; } = null!;
	}
}
