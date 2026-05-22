using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Receviving.ReceivingExceptions
{
	public class ReceiptAlreadyVerifyDomainException : DomainException
	{
		public Guid ReceiptId {get;}
		public ReceiptAlreadyVerifyDomainException(Guid receiptId)
			: base($"Receipt {receiptId} already verified. Operation prohibited.")
		{
			ReceiptId = receiptId;
		}
	}
}
