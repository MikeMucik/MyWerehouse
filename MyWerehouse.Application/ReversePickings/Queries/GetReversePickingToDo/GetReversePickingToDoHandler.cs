using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo
{
	public class GetReversePickingToDoHandler(IReversePickingRepo reversePickingRepo,
		IPalletRepo palletRepo,
		IProductRepo productRepo,
		IMapper mapper) : IRequestHandler<GetReversePickingToDoQuery, AppResult<ReversePickingDetailsDTO>>
	{
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<ReversePickingDetailsDTO>> Handle(GetReversePickingToDoQuery query, CancellationToken ct)
		{
			var reversePickingTask = await _reversePickingRepo.GetReversePickingAsync(query.PickingTaskId);
			var pickingTask = reversePickingTask.PickingTask;
			var reversePickingDTO = _mapper.Map<ReversePickingDTO>(reversePickingTask);
			var remainingQuantity = pickingTask.PickedQuantity;
			var product = await _productRepo.GetProductByIdAsync(pickingTask.ProductId);
			if (product == null) return AppResult<ReversePickingDetailsDTO>.Fail($"Produkt o numerze {pickingTask.ProductId} nie istnieje.", ErrorType.NotFound);

			if (product.CartonsPerPallet == 0) return AppResult<ReversePickingDetailsDTO>.Fail($"Produkt {pickingTask.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt", ErrorType.Conflict);
			
			var sourcePalletId = pickingTask.VirtualPallet.PalletId;
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(sourcePalletId);
			if (sourcePallet == null) return AppResult<ReversePickingDetailsDTO>.Fail($"Paleta o numerze {pickingTask.VirtualPallet.PalletId} nie istnieje.", ErrorType.NotFound);
			// czy można dołączyć do palety z której pobierano
			bool addSource = false;
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				addSource = true;
			}
			//czy istnieje paleta/y do której można dodać
			var filtr = new PalletSearchFilter
			{
				ProductId = pickingTask.ProductId,
				BestBefore = pickingTask.BestBefore,
			};
			var palletsWithProduct = _palletRepo.GetPalletsByFilter(filtr)//TODO reversePallet + reversePallet ;) repo
				.Where(p =>
				//p.ReceiptId != 0 || 
				p.Receipt != null)
				.Where(p => p.Status == PalletStatus.Available || p.Status == PalletStatus.ToPicking);			
			var notFullPallets = await palletsWithProduct
				.Where(p => p.ProductsOnPallet.Single().Quantity < product.CartonsPerPallet)
				.OrderByDescending(p => p.ProductsOnPallet.Single().Quantity)
				.ToListAsync(ct);
			//lista palet do których dodamy
			bool canAddedtoExist = false;
			var listPalletsToAdd = new List<Pallet>();
			foreach (var pallet in notFullPallets)
			{
				if (remainingQuantity <= 0) break;
				var palletLackQuantity = product.CartonsPerPallet - pallet.ProductsOnPallet.Single().Quantity;
				remainingQuantity -= palletLackQuantity;
				listPalletsToAdd.Add(pallet);
				if (remainingQuantity <= 0)
				{
					canAddedtoExist = true;
					break;
				}
			}
			var respone = new ReversePickingDetailsDTO
			{
				AddToNewPallet = true,
				CanReturnToSource = addSource,
				CanAddToExistingPallet = canAddedtoExist,//muszą być oba lub żadne
				ListPalletsToAdd = listPalletsToAdd,//muszą być oba lub żadne
				ReversePickingDTO = reversePickingDTO
			};
			return AppResult<ReversePickingDetailsDTO>.Success(respone);
		}
	}
}
