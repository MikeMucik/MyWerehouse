using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Receiving.ReceivingExceptions
{
	public class CannotCancelReceiptDomainException : DomainException
	{
		public Guid Id { get;}
		public int ReceiptNumber { get; }
		public CannotCancelReceiptDomainException(Guid id, int receiptNumber)
			: base($"It is not possible to cancel the receipt{id}, {receiptNumber} of a pallet already in the warehouse circulation.")
		{
			Id = id;
			ReceiptNumber = receiptNumber;
		}
	}
}
