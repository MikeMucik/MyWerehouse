using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ViewModels.IssueModels
{
	public class ProductOnPalletIssueDTO
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string SKU { get; set; }
		public int Quantity { get; set; }
		public DateOnly? BestBefore { get; set; }
	}
}
