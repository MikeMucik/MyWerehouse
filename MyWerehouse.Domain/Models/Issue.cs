using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Domain.Models
{
	public class Issue
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime IssueDateTime { get; set; }
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public string? PerformedBy { get; set; } // opcjonalnie: user												
	}
}
