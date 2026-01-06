using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Exceptions.BuisnessRuleException;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class ReceiptException : BusinessRuleException
	{
		public int ReceiptId { get; set; }
		public ReceiptException(int receiptId)
			: base($"Przyjęcie o numerze {receiptId} nie zostało znalezione.")
		{
			ReceiptId = receiptId;
		}
		public ReceiptException(string message) : base(message) { }

		//public ReceiptException(string message, Exception inner) : base(message, inner) { }
	}
}
