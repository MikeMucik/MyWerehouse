using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class IssueItem
	{
		public int Id { get; set; }
		public int IssueId { get; set; }
		public virtual Issue Issue { get; set; }
		public int ProductId { get; set; }
		public virtual Product Product { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
