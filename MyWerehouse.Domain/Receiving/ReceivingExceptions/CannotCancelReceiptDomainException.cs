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
			: base($"Cannot cancel receipt {id}, {receiptNumber}, pallets are already in stock circulation.")
		{
			Id = id;
			ReceiptNumber = receiptNumber;
		}
	}
}
