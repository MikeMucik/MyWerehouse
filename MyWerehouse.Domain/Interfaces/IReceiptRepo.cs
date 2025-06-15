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
		Task AddReceiptAsync(Receipt receipt);
		void DeleteReceipt(int id);
		Task DeleteReceiptAsync(int id);
		void UpdateReceipt(Receipt receipt);
		Task UpdateReceiptAsync(Receipt receipt);
		Receipt? GetReceiptById(int id);
		Task<Receipt?> GetReceiptByIdAsync(int id);
		IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter);
	}
}
