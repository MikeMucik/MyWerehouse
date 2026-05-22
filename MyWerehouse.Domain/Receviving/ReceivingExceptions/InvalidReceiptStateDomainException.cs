using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Receviving.ReceivingExceptions
{
	public class InvalidReceiptStateDomainException : DomainException
	{
		public Guid ReceiptId { get; }
		public ReceiptStatus ReceiptStatus { get; }
		public InvalidReceiptStateDomainException(Guid receiptId, ReceiptStatus receiptStatus)
			: base($"Operation prohibited for {receiptId}. Incorrect status {receiptStatus}.")
		{
			ReceiptId = receiptId;
			ReceiptStatus = receiptStatus;
		}
	}
}