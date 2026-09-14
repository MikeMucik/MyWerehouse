using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IReceiptRepo
	{
		void AddReceipt(Receipt receipt);
		void DeleteReceipt(Receipt receipt);
		Task<Receipt?> GetReceiptByIdAsync(Guid id, CancellationToken ct);
		Task<Receipt?> GetReceipForCancelByIdAsync(Guid id, CancellationToken ct);
		Task<int> GetNextNumberOfReceipt(CancellationToken ct);
		Task<bool> HasReceiptClient(int clientId, CancellationToken ct);
		Task<bool> HasReceiptProduct(Guid productId, CancellationToken ct);
	}
}
