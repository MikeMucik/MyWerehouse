using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Products.Services
{
	public class GetNumberPalletsAndRestService : IGetNumberPalletsAndRestService
	{
		private readonly IProductRepo _repo;
		public GetNumberPalletsAndRestService(IProductRepo productRepo)
		{
			_repo = productRepo;
		}
		//zostawiam do rozwoju innych polityk
		//public async Task<AssignPallestResult> GetNumbers(Guid productId, int amountUnits)
		//{
		//	var product = await _repo.GetProductByIdAsync(productId);
		//	if (product.CartonsPerPallet == 0)
		//		return AssignPallestResult.Fail($"Produkt {productId} nie ma ustawionej liczby kartonów na paletę. Popraw produkt.");
		//	var amountCarOnPallet = product.CartonsPerPallet;
		//	var amountPallets = amountUnits/ amountCarOnPallet;
		//	var rest = amountUnits% amountCarOnPallet;
		//	return AssignPallestResult.Ok(amountPallets, rest);
		//}
		public async Task<int> GetBackOnlyFullPallets(Guid productId, int amountUnits)
		{
			var product = await _repo.GetProductByIdAsync(productId);
			var amountCarOnPallet = product.CartonsPerPallet;//product sprawdzony w metodzie wyżej
			var amountPallets = amountUnits / amountCarOnPallet;			
			return amountPallets;
		}
	}
}
