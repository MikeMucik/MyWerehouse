using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Domain.Pallets.Models
{
	public class Pallet
	{
		public string Id { get; set; }
		public DateTime DateReceived { get; set; }
		public int LocationId { get; set; }
		public virtual Location Location { get; set; }
		public PalletStatus Status { get; set; } = 0;
		public virtual ICollection<ProductOnPallet> ProductsOnPallet { get; set; } = new HashSet<ProductOnPallet>();
		public virtual ICollection<PalletMovement> PalletMovements { get; set; } = new List<PalletMovement>();
		public int? ReceiptId {  get; set; }
		public virtual Receipt? Receipt { get; set; }
		public int? IssueId { get; set; }
		public virtual Issue? Issue { get; set; }
		[Timestamp]
		public byte[] RowVersion { get; set; }
	}
}
