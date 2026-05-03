using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Warehouse.Models;


namespace MyWerehouse.Domain.Picking.Models
{
	public class VirtualPallet
	{
		public Guid Id { get; private set; }
		public Guid PalletId { get; private set; }
		public Pallet Pallet { get;private set; }
		public int InitialPalletQuantity { get; private set; }
		public int LocationId { get; private set; }//needed??
		public Location Location { get; private set; }//needed??
		public DateTime DateMoved { get;private set; }
		public ICollection<PickingTask> PickingTasks { get; private set; } //= new List<PickingTask>();
		public ICollection<HistoryPicking> HistoryPicking { get;private set; } = new List<HistoryPicking>();
		[NotMapped]
		public int RemainingQuantity => InitialPalletQuantity - (PickingTasks?.Sum(a=>a.RequestedQuantity) ?? 0);
		private VirtualPallet() { }

		private VirtualPallet(Guid palletId, int initialQuantity, int locationId)
		{
			Id = Guid.NewGuid();
			PalletId = palletId;
			InitialPalletQuantity = initialQuantity;
			LocationId = locationId;
			DateMoved = DateTime.UtcNow;
		}

		public static VirtualPallet Create(Guid palletId, int initialQuantity, int locationId)
			=> new VirtualPallet(palletId, initialQuantity, locationId);

		private VirtualPallet(Guid id, Guid palletId, int initialQuantity, int locationId, DateTime date)
		{
			Id = id;
			PalletId = palletId;
			InitialPalletQuantity = initialQuantity;
			LocationId = locationId;
			DateMoved = date;
		}

		public static VirtualPallet CreateForSeed(Guid id, Guid palletId, int initialQuantity, int locationId, DateTime dateMoved)
			=> new VirtualPallet(id, palletId, initialQuantity, locationId, dateMoved);
		public void ChangeToAvailable(string userId, string snapShot)
		{
			var pickingTasks = this.PickingTasks;
			if (!(pickingTasks.Any(t => t.PickingStatus == PickingStatus.Allocated)))
			{
				Pallet.ChangeStatus(PalletStatus.Available);
				Pallet.AddHistory(Histories.Models.ReasonMovement.ReversePicking, userId, snapShot);
			}
		}
	}
}
