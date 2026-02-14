using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Picking.Models
{
	public class PickingTask : AggregateRoots
	{
		public int Id { get; set; }
		public int VirtualPalletId { get; set; }
		public VirtualPallet VirtualPallet { get; set; }
		public int IssueId { get; set; }
		public Issue Issue { get; set; }
		public int RequestedQuantity { get; set; }
		public PickingStatus PickingStatus { get; set; }


		public int ProductId { get; set; } //
		public DateOnly? BestBefore {  get; set; }
		public string? PickingPalletId { get; set; }
		public Pallet? PickingPallet { get; set; }

		public DateOnly PickingDay { get; set; }
		public int PickedQuantity { get; set; }
		public void MarkPicked(string pickingPalletId)
		{
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new InvalidOperationException("PickingTask already picked.");

			if (string.IsNullOrWhiteSpace(pickingPalletId))
				throw new ArgumentException("Picking pallet id is required.");
			PickedQuantity = RequestedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.Picked;
		}
		public void MarkPartiallyPicked(string pickingPalletId, int pickedQuantity)
		{
			if (PickingStatus == PickingStatus.Picked || PickingStatus == PickingStatus.PickedPartially)
				throw new InvalidOperationException("PickingTask already picked.");

			if (string.IsNullOrWhiteSpace(pickingPalletId))
				throw new ArgumentException("Picking pallet id is required.");
			PickedQuantity = pickedQuantity;
			PickingPalletId = pickingPalletId;
			PickingStatus = PickingStatus.PickedPartially;
		}
	}
}
