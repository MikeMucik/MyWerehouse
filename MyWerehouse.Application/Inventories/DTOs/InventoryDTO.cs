
using MyWerehouse.Domain.Inventories.Models;

namespace MyWerehouse.Application.Inventories.DTOs
{
	public class InventoryDTO 
	{
		public Guid ProductId { get; set; }
		public int Quantity { get; set; }
		public DateTime LastUpdated { get; set; }		
	}
}
