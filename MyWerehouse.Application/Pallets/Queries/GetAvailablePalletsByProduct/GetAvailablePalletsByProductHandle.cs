using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Queries.GetAvailablePalletsByProduct
{
	public class GetAvailablePalletsByProductHandle : IRequestHandler<GetAvailablePalletsByProductQuery, List<Pallet>>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly WerehouseDbContext _dbContext;
		public GetAvailablePalletsByProductHandle(IPalletRepo palletRepo, WerehouseDbContext dbContext)
		{
			_palletRepo = palletRepo;
			_dbContext = dbContext;
		}
		public async Task<List<Pallet>> Handle(GetAvailablePalletsByProductQuery request, CancellationToken ct)
		{
			//Do upadate
			//var tracked = _werehouseDbContext.ChangeTracker.Entries<Pallet>()
			//	.Where(e => e.Entity.Status == PalletStatus.Available
			//			&& e.Entity.ProductsOnPallet.Any(prod => prod.ProductId == request.ProductId
			//			&& (prod.BestBefore >= request.BestBefore)))
			//	.Select(e => e.Entity)
			//	.ToList();
			//var trackedIds = tracked.Select(p => p.Id).ToHashSet();

			//var fromDb = await _palletRepo.GetAvailablePallets(request.ProductId, request.BestBefore)
			//	.Where(p => !trackedIds.Contains(p.Id))
			//	.ToListAsync(cancellationToken: cancellationToken);

			//return tracked.Concat(fromDb)
			//	.OrderByDescending(p => p.ProductsOnPallet.First().BestBefore)
			//	.ToList();

			//var pallets = _palletRepo.GetAvailablePallets(request.ProductId, request.BestBefore)
			//	.OrderByDescending(p => p.ProductsOnPallet.First().BestBefore);

			//var numberToTake = request.Reserved;
			//var amountCartoonsTookPallets = await pallets
			//	.Take(numberToTake)
			//	.SumAsync(q => q.ProductsOnPallet.First().Quantity, cancellationToken);
			//while (amountCartoonsTookPallets < request.NeededCartoons)
			//{
			//	numberToTake++;
			//	amountCartoonsTookPallets = await pallets
			//	.Take(numberToTake)
			//	.SumAsync(q => q.ProductsOnPallet.First().Quantity, cancellationToken);
			//}

			//var palletsToTake = await pallets.Take(numberToTake).ToListAsync(cancellationToken);
			//foreach (var pallet in palletsToTake)
			//{
			//	pallet.Status = PalletStatus.InTransit;
			//}
			//return palletsToTake;

			// 1. Pobranie palet z repo — już posortowane po BestBefore DESC
			// ─────────────────────────────────────────────────────────────
			var palletsQuery = _palletRepo
				.GetAvailablePallets(request.ProductId, request.BestBefore)
				 // .UseLocking()
				.Select(p => new
				{
					Pallet = p,
					Qty = p.ProductsOnPallet
						.Where(x => x.ProductId == request.ProductId)
						.Select(x => x.Quantity)
						.FirstOrDefault(),
					BestBefore = p.ProductsOnPallet
						.Where(x => x.ProductId == request.ProductId)
						.Select(x => x.BestBefore)
						.FirstOrDefault()
				})
				.OrderByDescending(x => x.BestBefore);

			var palletList = await palletsQuery.ToListAsync(ct);

			// 2. Jeśli nie ma żadnych palet
			// ─────────────────────────────────────────────────────────────
			if (palletList.Count == 0)
				return new List<Pallet>();

			// 3. Dobieranie palet aż do uzyskania wymaganego NeededCartoons
			// ─────────────────────────────────────────────────────────────
			int taken = request.Reserved;
			int sum = palletList.Take(taken).Sum(x => x.Qty);

			while (sum < request.NeededCartoons && taken < palletList.Count)
			{
				taken++;
				sum = palletList.Take(taken).Sum(x => x.Qty);
			}

			// 4. Ostateczny wybór palet
			// ─────────────────────────────────────────────────────────────
			var selected = palletList
				.Take(taken)
				.Select(x => x.Pallet)
				.ToList();

			// 5. Ustawienie statusów
			// ─────────────────────────────────────────────────────────────
			foreach (var pallet in selected)
				pallet.Status = PalletStatus.InTransit;

			return selected;
		}
	}
}
