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
	public class ReceiptRepo : IReceiptRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public ReceiptRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}		
		public async Task AddReceiptAsync(Receipt receipt)
		{
			await _werehouseDbContext.Receipts.AddAsync(receipt);		
		}		
		public async Task DeleteReceiptAsync(int id)
		{
			var receipt =await _werehouseDbContext.Receipts.FindAsync(id);
			if (receipt != null)
			{
				_werehouseDbContext.Receipts.Remove(receipt);
				await _werehouseDbContext.SaveChangesAsync(true);
			}
		}		
		public async Task<Receipt?> GetReceiptByIdAsync(int id)
		{
			return await _werehouseDbContext.Receipts
				.Include(r => r.Pallets)
					.ThenInclude(pr => pr.ProductsOnPallet)
				.SingleOrDefaultAsync(r => r.Id == id);
		}
		public IQueryable<Receipt> GetReceiptByFilter(IssueReceiptSearchFilter filter)
		{
			var result = _werehouseDbContext.Receipts
				//.Include(r=>r.Client)
				//.Include(r=>r.Pallets)
				//	.ThenInclude(rp=>rp.ProductsOnPallet)
				//		.ThenInclude(rpp=>rpp.Product)
				.AsQueryable()
				;
			if (filter.ClientId > 0)
			{
				result = result.Where(i => i.ClientId == filter.ClientId);
			}
			if (filter.ClientName != null)
			{
				result = result.Where(i => i.Client.Name == filter.ClientName);
			}
			if (filter.ProductId > 0)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.ProductId == filter.ProductId)));
			}
			if (filter.ProductName != null)
			{
				result = result.Where(i => i.Pallets.Any(ip => ip.ProductsOnPallet.Any(ipp => ipp.Product.Name == filter.ProductName)));
			}
			if (filter.DateTimeStart != null)
			{
				var start = filter.DateTimeStart;
				var end = filter.DateTimeEnd ?? DateTime.Now;

				result = result.Where(i => i.ReceiptDateTime >= start && i.ReceiptDateTime <= end);
			}
			if (filter.UserId != null)
			{
				result = result.Where(i => i.PerformedBy == filter.UserId);
			}
			return result;			
		}			
	}
}
