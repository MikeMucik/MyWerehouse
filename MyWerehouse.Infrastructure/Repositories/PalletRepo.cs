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
	public class PalletRepo : IPalletRepo
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		public PalletRepo(WerehouseDbContext werehouseDbContext)
		{
			_werehouseDbContext = werehouseDbContext;
		}
		public string AddPallet(Pallet pallet)
		{
			_werehouseDbContext.Pallets.Add(pallet);
			//_werehouseDbContext.SaveChanges();
			return pallet.Id;
		}
		public async Task<string> AddPalletAsync(Pallet pallet)
		{
			await _werehouseDbContext.Pallets.AddAsync(pallet);
			//await _werehouseDbContext.SaveChangesAsync();
			return pallet.Id;
		}
		public void DeletePallet(string palletId)
		{
			var pallet = _werehouseDbContext.Pallets.Find(palletId);
			if (pallet != null)
			{
				_werehouseDbContext.Pallets.Remove(pallet);
				_werehouseDbContext.SaveChanges();
			}
		}
		public async Task DeletePalletAsync(string palletId)
		{
			var pallet = await _werehouseDbContext.Pallets.FindAsync(palletId);
			if (pallet != null)
			{
				_werehouseDbContext.Pallets.Remove(pallet);
				await _werehouseDbContext.SaveChangesAsync();
			}
		}
		public void UpdatePallet(Pallet pallet)
		{
			var entry = _werehouseDbContext.Entry(pallet);
			if (entry.State == EntityState.Deleted || entry.State == EntityState.Unchanged)
			{
				entry.State = EntityState.Modified;
			}
			_werehouseDbContext.SaveChanges();
		}
		public async Task UpdatePalletAsync(Pallet pallet)
		{
			_werehouseDbContext.Attach(pallet);
			if (pallet.LocationId > 0)
			{
				_werehouseDbContext.Entry(pallet).Property(nameof(pallet.LocationId)).IsModified = true;
			}
			if (pallet.IssueId > 0)
			{
				_werehouseDbContext.Entry(pallet).Property(nameof(pallet.IssueId)).IsModified = true;
			}
			if (pallet.ReceiptId > 0)
			{
				_werehouseDbContext.Entry(pallet).Property(nameof(pallet.ReceiptId)).IsModified = true;
			}
			_werehouseDbContext.Entry(pallet).Property(nameof(pallet.Status)).IsModified = true;

			await _werehouseDbContext.SaveChangesAsync();
		}
		public Pallet? GetPalletById(string palletId)
		{
			return _werehouseDbContext.Pallets
				.Include(p=>p.ProductsOnPallet)
				.Include(p=>p.PalletMovements)
				//.Include(p=>p.Location)
				//.Include(p=>p.Receipt)
				//.Include(p=>p.Issue)
				.FirstOrDefault(p => p.Id == palletId);
		}
		public async Task<Pallet?> GetPalletByIdAsync(string palletId)
		{
			return await _werehouseDbContext.Pallets.FirstOrDefaultAsync(p => p.Id == palletId);
		}
		public Pallet GetPalletWithProducts(string palletId)
		{
			return _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.First(p => p.Id == palletId);
		}
		public async Task<Pallet> GetPalletWithProductsAsync(string palletId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == palletId);
		}
		public Pallet GetPalletWithHistory(string palletId)
		{
			return _werehouseDbContext.Pallets
				.Include(p => p.PalletMovements)
				.First(p => p.Id == palletId);
		}
		public async Task<Pallet> GetPalletWithHistoryAsync(string palletId)
		{
			return await _werehouseDbContext.Pallets
				.Include(p => p.PalletMovements)
				.FirstAsync(p => p.Id == palletId);
		}
		public IQueryable<Pallet> GetPalletsByBasedFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
						.ThenInclude(ppr => ppr.ReceiptList)
				.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.AsQueryable();
			if (filter.ProductId > 0)
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
			return result;
		}
		public IQueryable<Pallet> GetPalletsByClientFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
				.Include(p => p.Location)
				.Include(p => p.Issue)
					.ThenInclude(pp => pp.Client)
				.Include(p => p.Receipt)
					.ThenInclude(pp => pp.Client)
				.AsQueryable();
			if (filter.ClientIdIn != null)
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.ClientId == filter.ClientIdIn);
			}
			if (filter.ClientIdOut != null)
			{
				result = result.Where(p => p.Issue != null && p.Issue.ClientId == filter.ClientIdOut);
			}
			return result;
		}
		public IQueryable<Pallet> GetPalletsByUser(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
				.Include(p => p.Location)
				.Include(p => p.Issue)
					.ThenInclude(pp => pp.Client)
				.Include(p => p.Receipt)
					.ThenInclude(pp => pp.Client)
				.AsQueryable();
			if (!string.IsNullOrEmpty(filter.ReceiptUser))
			{
				result = result.Where(p => p.Receipt != null && p.Receipt.PerformedBy == filter.ReceiptUser);
			}
			return result;
		}
		public IQueryable<Pallet> GetAvailablePallets(int productId, DateOnly minBestBefore)
		{
			var pallets = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.Where(p =>
				p.ProductsOnPallet.Any(pp => pp.ProductId
				== productId && pp.BestBefore >= minBestBefore
				&& pp.Pallet.Status == PalletStatus.Available))
				.OrderBy(p => p.ProductsOnPallet
					.Where(pp => pp.ProductId == productId)
					.Min(pp => pp.BestBefore))
				.ThenBy(p => p.LocationId);
			return pallets;
		}
		public void ClearPalletFromListReceipt(string palletId)
		{
			var pallet = _werehouseDbContext.Pallets
				.FirstOrDefault(p => p.Id == palletId);
			pallet.ReceiptId = 0;
			pallet.Status = PalletStatus.Available;//??
		}
		public void ClearPalletFromListIssue(string palletId)
		{
			var pallet = _werehouseDbContext.Pallets
				.FirstOrDefault(p => p.Id == palletId);
			pallet.IssueId = null;
			pallet.Status = PalletStatus.Available;
		}

		public void ChangePalletStatus(string palletId, PalletStatus palletStatus)
		{
			var pallet = _werehouseDbContext.Pallets
				.SingleOrDefault(p => p.Id == palletId);
			switch (palletStatus)
			{
				case PalletStatus.ToIssue:
					pallet.Status = PalletStatus.ToIssue;
					break;
				case PalletStatus.Damaged:
					pallet.Status = PalletStatus.Damaged;
					break;
				case PalletStatus.OnHold:
					pallet.Status = PalletStatus.OnHold;
					break;
				case PalletStatus.Loaded:
					pallet.Status = PalletStatus.Loaded;
					break;
				case PalletStatus.ToPicking:
					pallet.Status = PalletStatus.ToPicking;
					break;
				case PalletStatus.Archived:
					pallet.Status = PalletStatus.Archived;
					break;
				default:
					pallet.Status = PalletStatus.Available;
					break;
			}
		}

		public string GetNextPalletId()
		{
			var lastPallet = _werehouseDbContext.Pallets
				.Where(p=>p.Id.StartsWith("Q"))
				.OrderByDescending(p => p.Id)
				.FirstOrDefault();

			int lastNumber = 0;
			if(lastPallet != null&&int.TryParse(lastPallet.Id.Substring(1), out var parsed))
				{
					lastNumber = parsed;
				}
			string nextId = $"Q{(lastNumber + 1).ToString("D4")}";
			return nextId ;
		}
		public async Task<string> GetNextPalletIdAsync()
		{
			var lastPallet =await _werehouseDbContext.Pallets
				.Where(p => p.Id.StartsWith("Q"))
				.OrderByDescending(p => p.Id)
				.FirstOrDefaultAsync();

			int lastNumber = 0;
			if (lastPallet != null && int.TryParse(lastPallet.Id.AsSpan(1), out var parsed))
			{
				lastNumber = parsed;
			}
			string nextId = $"Q{(lastNumber + 1).ToString("D4")}";
			return nextId;
		}
		//zmiana koncepcji metoda zmiany statusów idzie do serwisu 
		// a będzie się wykonywać za pomoca update
		//public void MarkPalletAsHold(string id)
		//{
		//	var pallet = _werehouseDbContext.Pallets
		//		.FirstOrDefault(p => p.Id == id);
		//	pallet.Status = PalletStatus.OnHold;
		//}

		//public void MarkPalletAsAvailable(string id)
		//{
		//	var pallet = _werehouseDbContext.Pallets
		//		.FirstOrDefault(p => p.Id == id);
		//	pallet.Status = PalletStatus.Available;
		//}//dodaj więcej by wszytkie statusy i przerób które potrzebne na async - raczej większość
		//public void MarkPalletAsDamaged(string id)
		//{
		//	var pallet = _werehouseDbContext.Pallets
		//		.FirstOrDefault(p => p.Id == id);
		//	pallet.Status = PalletStatus.Damaged;
		//}
		//public void MarkPalletAsLoaded(string id)
		//{
		//	var pallet = _werehouseDbContext.Pallets
		//		.FirstOrDefault(p => p.Id == id);
		//	pallet.Status = PalletStatus.Loaded;
		//}
	}
}

//result = result.Where(p=>p.ProductsOnPallet.Any(pp=>pp.Product.Name == filter.ProductName));
//result = result.Where(p=> p.ProductsOnPallet!= null && p.ProductsOnPallet.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase));

//var existnigProductOnPallet = _werehouseDbContext.ProductOnPallet
//	.Where(p=>p.PalletId ==pallet.Id)
//	.ToList();
////jeśli istnieje w bazie ale zostaje usunięty podczas update
//foreach (var productOnPallet in existnigProductOnPallet)
//{
//	if(!pallet.ProductsOnPallet.Any(i=>i.Id == productOnPallet.Id))
//	{					
//		_werehouseDbContext.ProductOnPallet.Remove(productOnPallet);
//	}
//}		
//if(pallet.ProductsOnPallet != null)
//{
//	foreach (var productOnPallet in pallet.ProductsOnPallet)

//	{//jeśli jeszcze nie istnieje produkt na palecie
//		if (!existnigProductOnPallet.Any(i => i.Id == productOnPallet.Id))
//		{
//			productOnPallet.PalletId = pallet.Id;
//			_werehouseDbContext.ProductOnPallet.Add(productOnPallet);
//		}
//		else //istnieje i go updatujemy
//		{
//			var existing = existnigProductOnPallet.First(p=>p.Id == productOnPallet.Id);
//			existing.ProductId = productOnPallet.ProductId;
//			existing.Quantity = productOnPallet.Quantity;
//			existing.DateAdded = productOnPallet.DateAdded;
//			existing.BestBefore = productOnPallet.BestBefore;
//			//_werehouseDbContext.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
//		}
//	}
//}	