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
			_werehouseDbContext.SaveChanges();
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

		//public IQueryable<Pallet> GetAllPallets()
		//{
		//	return _werehouseDbContext.Pallets;
		//}

		public Pallet GetPalletWithProducts(string palletId)
		{
			return _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.First(p => p.Id == palletId);
		}

		public Pallet GetPalletWithHistory(string palletId)
		{
			return _werehouseDbContext.Pallets
				.Include(p => p.PalletMovements)
				.First(p => p.Id == palletId);
		}

		public void UpdatePallet(Pallet pallet)
		{
			_werehouseDbContext.Attach(pallet);
			if (pallet.LocationId > 0)
			{
				_werehouseDbContext.Entry(pallet).Property(nameof(pallet.LocationId)).IsModified = true;
			}
			_werehouseDbContext.Entry(pallet).Property(nameof(pallet.Status)).IsModified = true;

			_werehouseDbContext.SaveChanges();
		}

		//public IQueryable<Pallet> FindPalletsByProductId(int productId)
		//{
		//	var result = _werehouseDbContext.Pallets
		//		//.Include(p => p.ProductsOnPallet)
		//		.Where(i => i.ProductsOnPallet.Any(pp => pp.ProductId == productId));
		//	return result;
		//}

		//public IQueryable<Pallet> FindPalletsInLocation(int locationId)
		//{
		//	var result = _werehouseDbContext.Pallets
		//		//.Include(p => p.LocationId)
		//		.Where(l => l.LocationId == locationId);
		//	return result;
		//}

		public IQueryable<Pallet> GetPalletsByBasedFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
						.ThenInclude(ppr=>ppr.ReceiptList)
				.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.AsQueryable();
			if(filter.ProductId > 0)
			{
				result = result.Where(p=>p.ProductsOnPallet.Any(pp=>pp.ProductId == filter.ProductId));
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
			if(filter.PalletStatus.HasValue)
			{
				result = result.Where(p=>p.Status == filter.PalletStatus);
			}
			if (filter.BestBefore != null)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp=>
				pp.BestBefore == filter.BestBefore));
			}
			if (filter.DateAdded != null)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.DateAdded == filter.DateAdded));
			}			
			return result;
		}
		public IQueryable<Pallet> GetPalletsByClientFilter(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
						.ThenInclude(ppc => ppc.IssueList)
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
						.ThenInclude(ppc => ppc.ReceiptList)
				.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.AsQueryable();
			if(filter.ClientIdIn != null)
			{
				result = result.Where(p=>p.ProductsOnPallet.Any(pp=>
				pp.Product.ReceiptList.Any(r=>r.ClientId ==filter.ClientIdIn)));
			}
			if (filter.ClientIdIn != null)
			{
				result = result.Where(p => p.ProductsOnPallet.Any(pp =>
				pp.Product.IssueList.Any(r => r.ClientId == filter.ClientIdOut)));
			}
			return result;
		}

		public IQueryable<Pallet> GetPalletsByClientUser(PalletSearchFilter filter)
		{
			var result = _werehouseDbContext.Pallets
				.Include(p => p.ProductsOnPallet)
					.ThenInclude(pp => pp.Product)
						.ThenInclude(ppr => ppr.ReceiptList)
				.Include(p => p.PalletMovements)
				.Include(p => p.Location)
				.AsQueryable();
			if (!string.IsNullOrEmpty(filter.ReceiptUser))
			{
				result = result.Where(p =>
				p.ProductsOnPallet.Any(pp =>
				pp.Product != null && pp.Product.ReceiptList.Any(r => r.PerformedBy == filter.ReceiptUser)));
			}
			return result;
		}
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