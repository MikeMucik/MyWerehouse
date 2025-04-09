namespace MyWerehouse.Domain.Models
{
	public class Address
	{
		public int Id { get; set; }
		public string FullName { get; set; }
		//public int NameId { get; set; }
		public string Country { get; set; }
		public string City { get; set; }
		public string Region { get; set; }
		public string PostalCode { get; set; }
		public string StreetName { get; set; }
		public string StreetNumber { get; set; }
		public ICollection<Client> Clients { get; set; }

	}
}
