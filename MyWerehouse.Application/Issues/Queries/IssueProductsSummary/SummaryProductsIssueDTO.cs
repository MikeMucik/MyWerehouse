using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Issues.DTOs;

namespace MyWerehouse.Application.Issues.Queries.IssueProductsSummary
{
	public class SummaryProductsIssueDTO
	{
		public Guid Id { get; init; }
		public int IssueNumber { get; init; }
		public int ClientId { get; init; }
		public string PerformedBy { get; init; }
		public DateOnly DateToSend { get; init; }
		public List<IssueItemDTO> IssueItems { get; init; } = new List<IssueItemDTO>();

	}
}
