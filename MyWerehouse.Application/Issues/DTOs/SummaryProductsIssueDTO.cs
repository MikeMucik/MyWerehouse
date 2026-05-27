using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class SummaryProductsIssueDTO
	{
		public Guid Id { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public string PerformedBy { get; set; }
		public DateTime DateToSend { get; set; }
		public List<IssueItemDTO> IssueItems { get; set; } = new List<IssueItemDTO>();

	}
}
