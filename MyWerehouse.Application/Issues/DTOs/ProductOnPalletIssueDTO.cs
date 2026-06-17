using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class ProductOnPalletIssueDTO
	{
		public Guid ProductId { get; init; }
		public string ProductName { get; init; }
		public string SKU { get; init; }
		public int Quantity { get; init; }
		public DateOnly? BestBefore { get; init; }
	}
}
