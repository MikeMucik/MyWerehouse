using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.ReversePickings.Services
{
	public class PalletProductQuantityDTO
	{
		public Guid PalletId { get; init; }
		public string PalletNumber { get; init; }
		public Guid ProductId { get; init; }
		public string ProductName { get; init; }
		public string ProductSKU { get; init; }
		public int Quantity { get; init; }
	}
}
