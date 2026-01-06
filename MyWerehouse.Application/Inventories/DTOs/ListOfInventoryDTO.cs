using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Inventories.DTOs
{
	public class ListOfInventoryDTO
	{		
		public required List<InventoryDTO> InventoryDTOs { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int Count { get; set; }
	}
}
