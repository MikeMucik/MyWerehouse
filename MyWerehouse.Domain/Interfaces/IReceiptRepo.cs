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
		Task<Receipt?> GetReceiptByIdAsync(Guid id);
		Task<Receipt?> GetReceiptWithAllIncludesByIdAsync(Guid id);
		Task<Receipt?> GetReceipForCanceltByIdAsync(Guid id);
		IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter);
		Task<int> GetNextNumberOfReceipt();
	}
}
