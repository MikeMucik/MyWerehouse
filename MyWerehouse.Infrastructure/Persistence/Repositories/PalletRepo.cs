using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Infrastructure.Persistence.Repositories
{
	public class PalletRepo : IPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}

		public Guid AddPallet(Pallet pallet)
		{
			 _werehouseDbContext.Pallets.Add(pallet);
			return pallet.Id;
		}
		//public void DeletePallet(Pallet pallet)
		//{			
		//		_werehouseDbContext.Pallets.Remove(pallet);			
		//}
		public async Task<Pallet?> GetPalletByIdAsync(Guid palletId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				//.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.Include(p => p.Receipt)
				.Include(p => p.Issue)
				.FirstOrDefaultAsync(p => p.Id == palletId);
		}
		public IQueryable<Pallet> GetPalletsByFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(a => a.ProductsOnPallet)
				.Where(p => p.Status != PalletStatus.Archived);


			//if dla receiptNumber
			//if (filter.ProductId != null)
			if (filter.ProductId.HasValue)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp => pp.ProductId == filter.ProductId));
			}
			if (!string.IsNullOrWhiteSpace(filter.ProductName))
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product != null &&
				EF.Functions.Like(pp.Product.Name.ToLower(), $"%{filter.ProductName.ToLower()}%")));
			}
			if (filter.LocationId > 0)
			{
				result = result.Where(p => p.LocationId == filter.LocationId);
			}
			if (filter.PalletStatus.HasValue)
			{
				result = result.Where(p => p.Status == filter.PalletStatus);
			}
			if (filter.BestBefore != null || filter.BestBeforeTo != null)
			{
				var bestBeforeStart = filter.BestBefore ?? DateOnly.MinValue;
				var bestBeforeEnd = filter.BestBeforeTo ?? DateOnly.MaxValue;
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.BestBefore >= bestBeforeStart && pp.BestBefore <= bestBeforeEnd));
			}
			if (filter.StartDate != null)
			{
				var start = filter.StartDate.Value;
				var end = filter.EndDate ?? DateTime.Now;

				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.DateAdded >= start && pp.DateAdded <= end));
			}
			if (filter.ClientIdIn != null)
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.ClientId == filter.ClientIdIn);
			}
			if (filter.ClientIdOut != null)
			{
				result = result.Where(p => p.Issue != null && p.Issue.ClientId == filter.ClientIdOut);
			}
			if (!string.IsNullOrEmpty(filter.ReceiptUser))
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.PerformedBy == filter.ReceiptUser);
			}
			if (!string.IsNullOrEmpty(filter.IssueUser))
			{
				result = result.Where(p => p.Issue != null && p.Issue.PerformedBy == filter.IssueUser);//dodaj test
			}
			return result;
		}
		public IQueryable<Pallet> GetAvailablePallets(Guid productId, DateOnly? minBestBefore)
		{
			var pallets = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p =>
					(p.Status == PalletStatus.Available || p.Status == PalletStatus.InStock) &&
					p.ProductsOnPallet.Any(pp =>
						pp.ProductId == productId &&
						pp.BestBefore >= minBestBefore
					)
				)
				.OrderBy(p => p.ProductsOnPallet
					.Where(pp => pp.ProductId == productId)
					.Min(pp => pp.BestBefore))
				.ThenBy(p => p.LocationId)
				.ThenBy(p => p.DateReceived);
			return pallets;
		}
		
		//public void ClearPalletFromListIssue(Pallet pallet)
		//{			
		//	if (pallet is null) return;
		//	pallet.IssueId = null;
		//	pallet.Status = PalletStatus.Available;					
		//}

		//public void ChangePalletStatus(Guid palletId, PalletStatus palletStatus)
		//{
		//	var pallet =_werehouseDbContext.Pallets
		//		.Find(palletId);
		//	switch (palletStatus)
		//	{
		//		case PalletStatus.ToIssue:
		//			pallet.Status = PalletStatus.ToIssue;
		//			break;
		//		case PalletStatus.Damaged:
		//			pallet.Status = PalletStatus.Damaged;
		//			break;
		//		case PalletStatus.OnHold:
		//			pallet.Status = PalletStatus.OnHold;
		//			break;
		//		case PalletStatus.Loaded:
		//			pallet.Status = PalletStatus.Loaded;
		//			break;
		//		case PalletStatus.ToPicking:
		//			pallet.Status = PalletStatus.ToPicking;
		//			break;
		//		case PalletStatus.Archived:
		//			pallet.Status = PalletStatus.Archived;
		//			break;
		//		default:
		//			pallet.Status = PalletStatus.Available;
		//			break;
		//	}
		//}
		public async Task<string> GetNextPalletIdAsync()
		{
			var lastPallet = await _werehouseDbContext.Pallets
				.Where(static p => p.PalletNumber.StartsWith("Q"))
				.OrderByDescending(p => p.PalletNumber)
				.FirstOrDefaultAsync();

			//var lastPallet = await _werehouseDbContext.Pallets.MaxAsync(p => p.PalletNumber);

			int lastNumber = 0;
			if (lastPallet != null && int.TryParse(lastPallet.PalletNumber.AsSpan(1), out var parsed))
			//if (lastPallet != null && int.TryParse(lastPallet.AsSpan(1), out var parsed))
			{
				lastNumber = parsed;
			}
			string nextId = $"Q{(lastNumber + 1).ToString("D4")}";
			return nextId;
		}
		public async Task<Pallet> CheckOccupancyAsync(int locationId)
		{
			var pallet = await _werehouseDbContext.Pallets.FirstOrDefaultAsync(p => p.LocationId == locationId);
			return pallet;
		}

		public async Task<Pallet?> GetPickingPalletByIssueId(Guid issueId)
		{
		return	await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p => p.IssueId == issueId && p.Status == PalletStatus.Picking)
				.FirstOrDefaultAsync();
		}

		public async Task<List<Pallet>> GetPalletsByReceiptId(Guid reciptId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p=>p.ProductsOnPallet)
				.Include(m=>m.PalletMovements)
				.Where(p=>p.ReceiptId == reciptId)
				.ToListAsync();
		}


		//public async Task<bool> ExistsAsync(string palletId, int productId)
		//{

		//	return await _werehouseDbContext.ProductOnPallet
		//		.AnyAsync(p => p.PalletId == palletId && p.ProductId == productId);
		//}
	}	
}
