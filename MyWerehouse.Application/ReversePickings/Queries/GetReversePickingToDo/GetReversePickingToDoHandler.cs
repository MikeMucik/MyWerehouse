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
	public class GetReversePickingToDoHandler : IRequestHandler<GetReversePickingToDoQuery, AppResult<ReversePickingDetailsDTO>>
	{
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		public GetReversePickingToDoHandler(IReversePickingRepo reversePickingRepo,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IMapper mapper)
		{
			_reversePickingRepo = reversePickingRepo;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_mapper = mapper;
		}
		public async Task<AppResult<ReversePickingDetailsDTO>> Handle(GetReversePickingToDoQuery query, CancellationToken ct)
		{
			var reversePickingTask = await _reversePickingRepo.GetReversePickingAsync(query.PickingTaskId);
			var pickingTask = reversePickingTask.PickingTask;
			var reversePickingDTO = _mapper.Map<ReversePickingDTO>(reversePickingTask);
			var remainingQuantity = pickingTask.PickedQuantity;
			var product = await _productRepo.GetProductByIdAsync(pickingTask.ProductId);
			//?? throw new NotFoundProductException(pickingTask.ProductId);
			if (product == null) return AppResult<ReversePickingDetailsDTO>.Fail($"Produkt o numerze {pickingTask.ProductId} nie istnieje.", ErrorType.NotFound);

			if (product.CartonsPerPallet == 0)
			{
				return AppResult<ReversePickingDetailsDTO>.Fail($"Produkt {pickingTask.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt", ErrorType.Conflict);
			}
			var sourcePalletId = pickingTask.VirtualPallet.PalletId;
			var sourcePallet = await _palletRepo.GetPalletByIdAsync(sourcePalletId);
				//?? throw new NotFoundPalletException(nameof(sourcePalletId));
			if (sourcePallet == null) return AppResult<ReversePickingDetailsDTO>.Fail($"Paleta o numerze {pickingTask.VirtualPallet.PalletId} nie istnieje.", ErrorType.NotFound);

			bool addSource = false;
			if (sourcePallet.Status == PalletStatus.Available || sourcePallet.Status == PalletStatus.ToPicking)
			{
				addSource = true;
			}
			var filtr = new PalletSearchFilter
			{
				ProductId = pickingTask.ProductId,
				BestBefore = pickingTask.BestBefore,
			};
			var palletsWithSource = _palletRepo.GetPalletsByFilter(filtr)//TODO reversePallet + reversePallet ;)
				.Where(p =>
				//p.ReceiptId != 0 || 
				p.Receipt != null)
				//&& (r=> r.Status == PalletStatus.Available || r.Status == PalletStatus.ToPicking)
				.Where(p => p.Status == PalletStatus.Available || p.Status == PalletStatus.ToPicking);
			;

			var notFullPallets = await palletsWithSource
				.Where(p => p.ProductsOnPallet.Single().Quantity < product.CartonsPerPallet)
				.OrderByDescending(p => p.ProductsOnPallet.Single().Quantity)
				//.Except()
				.ToListAsync(ct);
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
				CanAddToExistingPallet = canAddedtoExist,
				ListPalletsToAdd = listPalletsToAdd,
				ReversePickingDTO = reversePickingDTO
			};
			return AppResult<ReversePickingDetailsDTO>.Success(respone);
		}
	}
}
