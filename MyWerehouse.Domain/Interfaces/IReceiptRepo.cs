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
		Task AddReceiptAsync(Receipt receipt);							
		Task<Receipt?> GetReceiptByIdAsync(int id);
		IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter);
	}
}
//Task DeleteReceiptAsync(int id);	