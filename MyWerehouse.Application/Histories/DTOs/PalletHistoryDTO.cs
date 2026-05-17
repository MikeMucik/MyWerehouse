using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Pagination;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class PalletHistoryDTO 
	{
		public Guid Id { get; set; }
		public string PalletNumber { get; set; }
		public DateTime DateReceived { get; set; }		
		public Guid? ReceiptId { get; set; }
		public int? ReceiptNumber { get; set; }
		public Guid? IssueId { get; set; }
		public int? IssueNumber { get; set; }
		public PagedResult<HistoryPalletDTO> PalletMovementsDTO { get; set; } = new PagedResult<HistoryPalletDTO>();
	}
}

