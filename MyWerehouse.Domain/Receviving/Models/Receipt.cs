using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.Receviving.Models
{
	public class Receipt
	{
		public int Id { get; set; }
		public int ClientId { get; set; }
		public virtual Client Client { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public virtual ICollection<HistoryReceipt> HistoryReceipt { get; set; } = new List<HistoryReceipt>();
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public int RampNumber { get; set; }
		public Receipt() { }
		public Receipt(int clientId, string performedBy, int rampNumber)
		{
			//if (clientId <= 0) throw new InvalidClientNumberException(clientId);
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdException(performedBy);
			//if (rampNumber <= 0 || rampNumber > 100) throw new NotFoundRampException(rampNumber);//dostępne rampy
			ClientId = clientId;
			PerformedBy = performedBy;
			ReceiptDateTime = DateTime.UtcNow;
			ReceiptStatus = ReceiptStatus.Planned;
			RampNumber = rampNumber;
		}
		public List<Pallet> VerifiedReceipt()
		{
			//if (ReceiptStatus != ReceiptStatus.PhysicallyCompleted)
			//	throw new InvalidReceiptException();
			var toReturn = Pallets.Where(p => p.Status == PalletStatus.Receiving).ToList();
			foreach (var pallet in toReturn)
			{
				pallet.Status = PalletStatus.InStock;
			}
			ReceiptStatus = ReceiptStatus.Verified;
			return toReturn;
		}
	}
}
