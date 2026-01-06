using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.ReservedPallet
{
	public class ReservedPalletHandler : IRequestHandler<ReservedPalletCommand, Pallet?>
	{
		private readonly IPalletRepo _palletRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		public ReservedPalletHandler(IPalletRepo palletRepo,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_werehouseDbContext = werehouseDbContext;
		}
		public async Task<Pallet?> Handle(ReservedPalletCommand request, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database
				.BeginTransactionAsync(IsolationLevel.Serializable, ct);
			try
			{
				var pallet = await _palletRepo
					.GetAvailablePallets(request.ProductId, request.BestBefore)
					.OrderBy(x => x.ProductsOnPallet.First().Quantity)
					.FirstOrDefaultAsync(ct);
				if (pallet == null)
				{
					return null;
				}

				pallet.Status = PalletStatus.InTransit;

				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return pallet;
			}
			catch (DbUpdateConcurrencyException)
			{
				await transaction.RollbackAsync(ct);
				// w SQLite (testy) może tu trafić drugi wątek
				return null; // albo retry logika
			}
			catch
			{
				return null;
			}
		}
	}
}
