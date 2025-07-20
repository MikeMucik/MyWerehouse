using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class ListPalletsToLoadDTO
	{
		public int IssueId { get; set; }
		public int ClientId { get; set; }
		public string ClientName { get; set; }
		public List<PalletToLoadDTO> Pallets { get; set; } = new List<PalletToLoadDTO>();
	}
}
