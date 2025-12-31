using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.PickingPallets.DTOs
{
	public class ProductToIssueDTO
		//model dla listy task - allocation 
	{
		public int Id { get; set; }
		public int ClientIdOut { get; set; }
		public int IssueId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }		
	}
}
