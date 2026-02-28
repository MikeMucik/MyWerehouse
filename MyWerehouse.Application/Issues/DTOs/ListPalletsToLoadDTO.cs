using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class ListPalletsToLoadDTO
	{
		public Guid IssueId { get; set; }
		public int IssueNumber { get; set; }
		public int ClientId { get; set; }
		public string ClientName { get; set; }
		public List<PalletToLoadDTO> Pallets { get; set; } = new List<PalletToLoadDTO>();
	}
}
