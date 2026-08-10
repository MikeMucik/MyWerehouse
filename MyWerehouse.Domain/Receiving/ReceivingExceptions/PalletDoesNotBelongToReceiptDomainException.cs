using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Receiving.ReceivingExceptions
{
	public class PalletDoesNotBelongToReceiptDomainException :DomainException
	{
		public Guid Id { get; }
		public int ReceiptNumber { get; }
		public Guid PalletId { get; }
		public  string PalletNumber { get; }
		public PalletDoesNotBelongToReceiptDomainException(Guid id, int receiptNumber, Guid palletId, string palletNumber)
			:base($"Pallet {palletId}, {palletNumber} is already assigned to other receipt")
		{
			Id = id;
			ReceiptNumber = receiptNumber;
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
	}
}
