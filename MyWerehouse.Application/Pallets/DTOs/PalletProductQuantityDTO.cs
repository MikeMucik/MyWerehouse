using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class PalletProductQuantityDTO
	{
		public Guid PalletId { get; set; }
		public string PalletNumber { get; set; }
		public Guid ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductSKU { get; set; }
		public int Quantity { get; set; }
	}
}
