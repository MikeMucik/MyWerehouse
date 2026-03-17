using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Clients.Models
{
	public class Client
	{
		public int Id { get; set; }
		public string Name { get; set; }		
		public string Email { get; set; }
		public string Description { get; set; }
		public string FullName { get; set; }
		public bool IsDeleted { get; set; } = false;
		public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
		public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
		public virtual ICollection<Issue> Issues { get; set; }	= new List<Issue>();
		//public const int MaxNameLength = 250;
		//public const int MaxDiscriptionLength = 500;
	}
}
