using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Picking.Queries.GetListIssueToPickingTree
{
	public class ProductOnPalletPickingDTO
	{
		public Guid ProductId { get; init;}
		public string SKU {  get; init;}//
		public int Quantity {  get; init;}
	}
}
