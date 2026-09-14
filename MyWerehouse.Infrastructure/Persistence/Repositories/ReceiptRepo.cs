using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Domain.Receiving.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class ReceiptRepo : IReceiptRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public ReceiptRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public void AddReceipt(Receipt receipt)
		{
			_werehouseDbContext.Receipts.Add(receipt);
		}

		public void DeleteReceipt(Receipt receipt)
		{
			_werehouseDbContext.Remove(receipt);
		}
		public async Task<Receipt?> GetReceiptByIdAsync(Guid id, CancellationToken ct)
		{
			return await _werehouseDbContext.Receipts
				.Include(r => r.Pallets)
					.ThenInclude(pr => pr.ProductsOnPallet)
				.Include(l => l.Pallets)
					.ThenInclude(l => l.Location)
				.FirstOrDefaultAsync(r => r.Id == id, ct);
		}
		
		public async Task<Receipt?> GetReceipForCancelByIdAsync(Guid id, CancellationToken ct)
		{
			return await _werehouseDbContext.Receipts
				.Include(p=>p.Pallets)
					.ThenInclude(p=>p.PalletHistory)
				.Include(p => p.Pallets)
					.ThenInclude(p => p.Location)
				.FirstOrDefaultAsync(r => r.Id == id, ct);
		}
		
		public async Task<int> GetNextNumberOfReceipt(CancellationToken ct)
		{
			var number = await _werehouseDbContext.Receipts.MaxAsync(x => (int?)x.ReceiptNumber, ct) ??0;
			return number + 1;
		}

		public Task<bool> HasReceiptClient(int clientId, CancellationToken ct)
		{
			return _werehouseDbContext.Receipts
				.AnyAsync(x => x.ClientId == clientId, ct);
		}

		public Task<bool> HasReceiptProduct(Guid productId, CancellationToken ct)
		{
			return _werehouseDbContext.Receipts
				.AnyAsync(x => x.Pallets.Any(p => p.ProductsOnPallet.Any(pp=>pp.ProductId == productId)), ct);
		}
	}
}
