namespace MyWerehouse.Domain.Models
{
	public class Client
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int AdressId { get; set; }
		public virtual Address Adress { get; set; }
		public string Email { get; set; }
		public string Description { get; set; }
		public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
		public virtual ICollection<Issue> Issues { get; set; }	= new List<Issue>();
		
	}
}
