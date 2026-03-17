using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class HistoryReceiptRepo : IHistoryReceiptRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public HistoryReceiptRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public void AddHistoryReceipt(HistoryReceipt historyReceipt)
		{
			 _werehouseDbContext.HistoryReceipts.Add(historyReceipt);
		}

		public async Task AddHistoryReceiptAsync(HistoryReceipt historyReceipt, CancellationToken cancellationToken)
		{
			await _werehouseDbContext.AddAsync(historyReceipt, cancellationToken);
		}

		public IQueryable<HistoryReceipt> GetAllHistoryReceipt()
		{
			return _werehouseDbContext.HistoryReceipts				
				.AsQueryable();
		}
	}
}
