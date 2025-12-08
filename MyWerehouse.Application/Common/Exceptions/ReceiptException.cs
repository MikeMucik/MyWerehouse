using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Common.Exceptions
{
	public class ReceiptException : Exception
	{
		public int ReceiptId { get; set; }
		public ReceiptException(int receiptId)
			: base($"Przyjęcie o numerze {receiptId} nie zostało znalezione.")
		{
			ReceiptId = receiptId;
		}
		public ReceiptException(string message) : base(message) { }

		public ReceiptException(string message, Exception inner) : base(message, inner) { }
	}
}
