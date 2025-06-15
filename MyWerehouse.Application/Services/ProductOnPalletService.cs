using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Services
{
	public class ProductOnPalletService : IProductOnPalletService
	{
		private readonly IProductOnPalletRepo _productOnPalletRepo;
		private readonly IMapper _mapper;
		private readonly IPalletRepo _palletRepo;

		public ProductOnPalletService(
			IProductOnPalletRepo productOnPalletRepo,
			IMapper mapper,
			IPalletRepo palletRepo)
		{
			_productOnPalletRepo = productOnPalletRepo;
			_mapper = mapper;
			_palletRepo = palletRepo;
		}
			

		public void AddProductOnPalletReceipt(string palletId, int productId, int quantity)
		{
			throw new NotImplementedException();
		}

		public void AddProductToPalletPicking(string palletId, int productId, int quantity)
		{
			var pallet = _palletRepo.GetPalletById(palletId);
			if (pallet.ProductsOnPallet.Where(p => p.ProductId == productId).Any())
			{
				_productOnPalletRepo.IncreaseQuantityOnPallet(palletId, productId, quantity);
			}
			else
			{
				var productOnPallet = new Domain.Models.ProductOnPallet
				{
					PalletId = palletId,
					ProductId = productId,
					Quantity = quantity,
					DateAdded = DateTime.UtcNow
				};
				_productOnPalletRepo.AddProductToPallet(productOnPallet);
			}
		}

		public void ProductToPalletPicking(string palletIdFrom, string palletIdTo, int productId, int quantity)
		{
			var palletFrom = _palletRepo.GetPalletById(palletIdFrom);
			var palletTo = _palletRepo.GetPalletById(palletIdTo);
			if(_productOnPalletRepo.GetQuantity(palletIdFrom, productId) 
				>= _productOnPalletRepo.GetQuantity(palletIdTo, productId))
			{
				_productOnPalletRepo.DecreaseQuantityOnPallet(palletIdFrom, productId, quantity);
				if (palletTo.ProductsOnPallet.Where(p => p.ProductId == productId).Any())
				{
					_productOnPalletRepo.IncreaseQuantityOnPallet(palletIdTo, productId, quantity);
				}
				else
				{
					var productOnPallet = new Domain.Models.ProductOnPallet
					{
						PalletId = palletIdTo,
						ProductId = productId,
						Quantity = quantity,
						DateAdded = DateTime.UtcNow
					};
					_productOnPalletRepo.AddProductToPallet(productOnPallet);
				}
			}
			//if (palletFrom.ProductsOnPallet.Where(p => p.ProductId == productId).Any())
			//{
			//	_productOnPalletRepo.DecreaseQuantityOnPallet(palletIdFrom, productId, quantity);
			//}
			//if (palletTo.ProductsOnPallet.Where(p => p.ProductId == productId).Any())
			//{
			//	_productOnPalletRepo.IncreaseQuantityOnPallet(palletIdTo, productId, quantity);
			//}
			//else
			//{
			//	var productOnPallet = new Domain.Models.ProductOnPallet
			//	{
			//		PalletId = palletIdTo,
			//		ProductId = productId,
			//		Quantity = quantity,
			//		DateAdded = DateTime.UtcNow
			//	};
			//	_productOnPalletRepo.AddProductToPallet(productOnPallet);
			//}
			else
			{
				// gdy jest za mało na pierwszej palecie produktu i trzeba dobrać z drugiej
			}
		}
	}
}
