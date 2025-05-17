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
		void AddReceipt (Receipt receipt);
		void UpdateReceipt(Receipt receipt);
		void DeleteReceipt (int id);
		Receipt GetReceiptById (int id);
		IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter);
	}
}
