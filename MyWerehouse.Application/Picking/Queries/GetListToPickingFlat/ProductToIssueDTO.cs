using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListToPickingFlat
{
	public class ProductToIssueDTO
		//model dla listy task - pickingTask 
	{
		public int Id { get; init; }
		public int ClientIdOut { get; init; }
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public Guid ProductId { get; init; }
		public string SKU { get; init; }
		public int Quantity { get; init; }		
	}
}
