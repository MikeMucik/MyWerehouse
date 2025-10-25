using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Exceptions
{
	public class ReceiptNotFoundException : Exception
	{
		public int ReceiptId { get; set; }
		public ReceiptNotFoundException(int receiptId)
			: base($"Przyjęcie o numerze {receiptId} nie zostało znalezione.")
		{
			ReceiptId = receiptId;
		}
		public ReceiptNotFoundException(string message) : base(message) { }

		public ReceiptNotFoundException(string message, Exception inner) : base(message, inner) { }
	}
}
