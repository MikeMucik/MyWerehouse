using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public	 class IssueItemRepo //: IIssueItemRepo
	{
		//private readonly WerehouseDbContext _werehouseDbContext;
		//public IssueItemRepo(WerehouseDbContext werehouseDbContext)
		//{
		//	_werehouseDbContext = werehouseDbContext;
		//}

		//public void AddIssueItem(IssueItem issueItem)
		//{
		//	_werehouseDbContext.IssueItems.Add(issueItem);
		//}

		//public void DeleteIssueItem(IssueItem issue)
		//{
		//	_werehouseDbContext.IssueItems.Remove(issue);
		//}

		//public async Task<IssueItem> GetIssueItemAsync(int id)
		//{
		//	return await _werehouseDbContext.IssueItems.FirstOrDefaultAsync(a=>a.Id == id);
		//}

		//public async Task<int> GetQuantityByIssueAndProduct(Issue issue, Guid productId)
		//{
		//	var record = await _werehouseDbContext.IssueItems
		//		.FirstOrDefaultAsync(a => a.Issue == issue && a.ProductId == productId);
		//	return record != null ? record.Quantity : 0;			
		//}
	}
}
