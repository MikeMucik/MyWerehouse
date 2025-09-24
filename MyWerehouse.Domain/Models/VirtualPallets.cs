using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MyWerehouse.Domain.DomainExceptions;

namespace MyWerehouse.Domain.Models
{
	public class VirtualPallet
	{
		public int Id { get; set; }
		public string PalletId { get; set; }
		public Pallet Pallet { get; set; }
		public int IssueInitialQuantity { get; set; }
		public int LocationId { get; set; }
		public Location Location { get; set; }
		public DateTime DateMoved { get; set; }
		//public int IssueId { get; set; }
		public virtual ICollection<Allocation> Allocation { get; set; } = new List<Allocation>();
		public virtual ICollection<HistoryPicking> HistoryPicking { get; set; } = new List<HistoryPicking>();
		[NotMapped]
		public int RemainingQuantity => IssueInitialQuantity - (Allocation?.Sum(a=>a.Quantity) ?? 0);

		public VirtualPallet() { }

		public VirtualPallet(string palletId, int issueInitialQuantity, int locationId, DateTime dateMoved)
		{
			if (string.IsNullOrWhiteSpace(palletId))
				throw new DomainVirtualPalletException("Brak źródłowej palety");

			if (issueInitialQuantity <= 0)
				throw new DomainVirtualPalletException("Ilość początkowa dla pickingu musi być > 0");

			if (locationId <= 0)
				throw new DomainVirtualPalletException("Wirtualna paleta musi mieć przypisaną lokalizację");

			PalletId = palletId;
			IssueInitialQuantity = issueInitialQuantity;
			LocationId = locationId;
			DateMoved = dateMoved == default ? DateTime.UtcNow : dateMoved;
		}
	}
}
