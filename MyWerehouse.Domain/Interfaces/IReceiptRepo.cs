using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IReceiptRepo
	{		
		void AddReceipt(Receipt receipt);		
		void DeleteReceipt(Receipt receipt); 
		Task<Receipt?> GetReceiptByIdAsync(Guid id);
		Task<Receipt?> GetReceiptOnlyByIdAsync(Guid id);
		IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter);
		Task<int> GetNextNumberOfReceipt();
	}
}
