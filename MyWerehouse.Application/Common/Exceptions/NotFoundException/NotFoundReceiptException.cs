using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWerehouse.Application.Common.Exceptions.NotFoundException
{
	public class NotFoundReceiptException : NotFoundException
	{
		public int ReceiptId { get;}

		public NotFoundReceiptException(int receiptId):
			base($"Przyjęcie o numerze {receiptId} nie zostało znalezione.")
		{
			ReceiptId = receiptId;
		}
	}
}
