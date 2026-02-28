using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Domain.Picking.Models
{
	public class HandPickingTask
	{
		public int Id { get; set; }
		//public Guid Id { get; set; }		
		public Guid IssueId { get; set; }
		//public int PickinckTaskNumber { get; set; }
		public Issue Issue { get; set; }
		public int ProductId { get; set; }
		//public Product Product { get; set; }
		public int Quantity { get; set; }
		public PickingStatus PickingStatus { get; set; }
		public DateOnly? BestBefore { get; set; }
		public int PickedQuantity { get; set; }
		public DateTime CreateDate { get; set; }
		public void MarkHandPicked()
		{
			if (PickingStatus == PickingStatus.Picked)
				throw new InvalidOperationException("PickingTask already picked.");
						
			PickedQuantity = Quantity;
			PickingStatus = PickingStatus.Picked;
		}
		public void MarkHandPartiallyPicked( int pickedQuantity)
		{
			if (PickingStatus == PickingStatus.Picked)
				throw new InvalidOperationException("PickingTask already picked.");

			PickedQuantity = pickedQuantity;
			PickingStatus = PickingStatus.PickedPartially;
		}
	}
}
