using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundReceiptException : NotFoundException
	{
		public Guid ReceiptId { get;}
		public int ReceiptNumber { get;}

		public NotFoundReceiptException(Guid receiptId):
			base($"Przyjęcie o numerze {receiptId} nie zostało znalezione.")
		{
			ReceiptId = receiptId;
		}
		public NotFoundReceiptException(int receiptNumber) :
			base($"Przyjęcie o numerze {receiptNumber} nie zostało znalezione.")
		{
			ReceiptNumber = receiptNumber;
		}
	}
}
