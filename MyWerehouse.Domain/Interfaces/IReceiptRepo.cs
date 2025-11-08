using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Domain.Interfaces
{
	public interface IReceiptRepo
	{		
		void AddReceipt(Receipt receipt);
		void DeleteReceipt(Receipt receipt);
		Task<Receipt?> GetReceiptByIdAsync(int id);
		IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter);
	}
}
