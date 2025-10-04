namespace MyWerehouse.Domain.Models
{
	public class Client
	{
		public int Id { get; set; }
		public string Name { get; set; }		
		public string Email { get; set; }
		public string Description { get; set; }
		public string FullName { get; set; }
		public bool IsDeleted { get; set; } = false;
		public virtual ICollection<Address> Addresses { get; set; }
		public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
		public virtual ICollection<Issue> Issues { get; set; }	= new List<Issue>();		
	}
}
