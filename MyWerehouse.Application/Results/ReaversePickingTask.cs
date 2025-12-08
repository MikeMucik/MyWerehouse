using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Results
{
	public class ReaversePickingTask
	{
		public int Id { get; set; }
		public required string PickingPalletId { get; set; }
		public string? SourcePalletId { get; set; }
		public string? ResultPalletId { get; set; }
		public int ProductId { get; set; }
		public DateOnly BestBefore { get; set; }
		public int Quantity { get; set; }
	}
}
