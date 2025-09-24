using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Infrastructure.Repositories
{
	public class HistoryReceiptRepo : IHistoryReceiptRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public HistoryReceiptRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public async Task AddHistoryReceiptAsync(HistoryReceipt historyReceipt)
		{
			await _werehouseDbContext.HistoryReceipts.AddAsync(historyReceipt);
		}

		public IQueryable<HistoryReceipt> GetAllHistoryReceipt()
		{
			return _werehouseDbContext.HistoryReceipts				
				.AsQueryable();
		}
	}
}
