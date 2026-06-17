using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.Queries.LoadingIssueList
{
	public class ListPalletsToLoadDTO
	{
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public int ClientId { get; init; }
		public string ClientName { get; init; }
		public List<PalletToLoadDTO> Pallets { get; init; } = new List<PalletToLoadDTO>();
	}
}
