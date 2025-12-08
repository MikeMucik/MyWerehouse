using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Products.Queries.GetNumberUnitOnPallet
{
	public class GetNumberPalletsAndRestCommand :IRequestHandler<GetNumberPalletsAndRestQuery, AssignPallestResult>
	{
		private readonly IProductRepo _productRepo;
		public GetNumberPalletsAndRestCommand(IProductRepo productRepo)
		{
			_productRepo = productRepo;	
		}
		public async Task<AssignPallestResult> Handle (GetNumberPalletsAndRestQuery request, CancellationToken ct)
		{
			var numberUnitOnPallet = await _productRepo.GetProductByIdAsync(request.ProductId)
				?? throw new ProductException($"Produkt {request.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
			var number = numberUnitOnPallet.CartonsPerPallet;
			var amountPallets = request.AmountUnits / number;
			var rest = request.AmountUnits % number;
			return new AssignPallestResult ( amountPallets, rest );
		}
	}
}
