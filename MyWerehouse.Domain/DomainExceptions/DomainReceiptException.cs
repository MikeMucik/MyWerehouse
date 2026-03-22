using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Domain.DomainExceptions
{
	public class DomainReceiptException :DomainException
	{
		public string PalletNumber { get; set; }
		public Guid PalletId { get; set; }
		public int ReceiptId { get; set; }
		public string Message { get; set; }
		public DomainReceiptException(Guid palletId, string palletNumber)
			: base($"Błąd przy zapisie do bazy palety o numerze {palletId}.")
		{
			PalletNumber = palletNumber;
			PalletId = palletId;
		}
		//"Receipt operation failed"
		public DomainReceiptException(string message, int receiptId):
			base(message)
		{
			Message = message;
			ReceiptId = receiptId;
		}
		public DomainReceiptException(int receiptId)
			: base($"Błąd zapisu przyjęcia {receiptId}.")
		{
			ReceiptId = receiptId;
		}
		public DomainReceiptException(string message)
			: base(message)
		{
			
		}
	}
}
