using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ViewModels.AllocationModels
{
	public class AllocationDTO
	{
		public int AllocationId { get; set; }
		public int IssueId { get; set; }
		public required string SourcePalletId { get; set; }		
		public int ProductId { get; set; }
		public int RequestedQuantity { get; set; }
		public int PickedQuantity { get; set; }//faktyczna pobrana ilość
		public PickingStatus PickingStatus { get; set; }
		public DateOnly? BestBefore { get; set; }
	}
}
//public int ClientOut {  get; set; } ?