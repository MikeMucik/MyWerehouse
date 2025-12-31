using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.DomainExceptions;

namespace MyWerehouse.Domain.Models
{
	public class Receipt
	{
		public int Id { get; set; }
		public int ClientId {  get; set; }
		public virtual Client Client { get; set; }
		public DateTime ReceiptDateTime { get; set; }
		public virtual ICollection<Pallet> Pallets { get; set; } = new List<Pallet>();
		public virtual ICollection<HistoryReceipt> HistoryReceipt { get; set; } = new List<HistoryReceipt>();
		public string PerformedBy { get; set; } // opcjonalnie: user
		public ReceiptStatus ReceiptStatus { get; set; }
		public int RampNumber { get; set; }
		public Receipt() { }
		public Receipt (int clientId, string performedBy, int rampNumber)
		{
			if (clientId <= 0) throw new InvalidClientException(clientId);
			if (string.IsNullOrWhiteSpace(performedBy)) throw new InvalidUserIdException(performedBy);
			if (rampNumber <= 0 || rampNumber > 100) throw new InvalidRampException(rampNumber);
			ClientId = clientId;
			PerformedBy = performedBy;
			ReceiptDateTime = DateTime.UtcNow;
			ReceiptStatus = ReceiptStatus.Planned;
			RampNumber = rampNumber;
			
		}
	}
}
