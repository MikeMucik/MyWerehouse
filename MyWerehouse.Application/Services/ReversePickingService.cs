using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Services
{
	public class ReversePickingService : IReversePickingService
	{
		private readonly IIssueService _issueService;
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IAllocationRepo _allocationRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		public ReversePickingService(IIssueService issueService,
			IPickingPalletRepo pickingPalletRepo,
			IAllocationRepo allocationRepo,
			WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IProductRepo productRepo)
		{
			_issueService = issueService;
			_pickingPalletRepo = pickingPalletRepo;
			_allocationRepo = allocationRepo;
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
		}
		public async Task<List<ReversePickingResult>> CreateTaskToReversePickingAsync(string palletId)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			var listTasks = new List<ReversePickingResult>();
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(palletId)
					?? throw new PalletException(palletId);
				var issue = pallet.Issue;
				foreach (var item in pallet.ProductsOnPallet)
				{
					var filter = new PalletSearchFilter
					{
						ProductId = item.ProductId,
						BestBefore = item.BestBefore,
					};
					var addingPallets = _palletRepo.GetPalletsByFilter(filter);
					var palletToAdded = await addingPallets
							.Where(p => p.ReceiptId != null)//paleta z przyjęcia ma numer przyjęcia
							.OrderBy(q => q.ProductsOnPallet.First().Quantity)//paleta z przyjęcia ma tylko jeden asortyment
							.FirstOrDefaultAsync()
							?? throw new PalletException("Brak palety do dodania.");
					var product = await _productRepo.GetProductByIdAsync(item.ProductId)
						?? throw new ProductException($"Produkt {item.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
					var numberOfCartoons = product.CartonsPerPallet;
					if (palletToAdded.ProductsOnPallet.First().Quantity < numberOfCartoons)
					{
						palletToAdded.Status = PalletStatus.ReversePicking;
					}
					listTasks.Add(ReversePickingResult.Ok("Stworzono zadanie dekompletacyjne", item.ProductId, palletId));
				}
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (IssueException ie)
			{
				await transaction.RollbackAsync();
				listTasks.Add(ReversePickingResult.Fail(ie.Message));
			}
			catch (PalletException pe)
			{
				await transaction.RollbackAsync();
				listTasks.Add(ReversePickingResult.Fail(pe.Message));
			}
			catch (ProductException proe)
			{
				await transaction.RollbackAsync();
				listTasks.Add(ReversePickingResult.Fail(proe.Message));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			}
			return listTasks;
		}
	}
}
