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
		public string PalletId { get; set; }
		public int ReceiptId { get; set; }
		public DomainReceiptException(string palletId)
			: base($"Błąd przy zapisie do bazy palety o numerze {palletId}.")
		{
			PalletId = palletId;
		}
		public DomainReceiptException(int receiptId)
			: base($"Błąd zapisu przyjęcia {receiptId}.")
		{
			ReceiptId = receiptId;
		}
	}
}
