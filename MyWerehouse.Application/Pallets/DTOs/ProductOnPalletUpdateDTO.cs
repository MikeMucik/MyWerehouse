using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletUpdateDTO
	{
		public Guid ProductId { get; init; }
		public Guid PalletId { get; init; }
		public int Quantity { get; init; }
		public DateTime DateAdded { get; init; }
		public DateOnly? BestBefore { get; init; }
	}
}
