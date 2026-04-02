using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreateNewPalletHandler(WerehouseDbContext werehouseDbContext,
		IPalletRepo palletRepo,
		IMapper mapper,
		IProductRepo productRepo,
		ILocationRepo locationRepo) : IRequestHandler<CreateNewPalletCommand, AppResult<Unit>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMapper _mapper = mapper;
		private readonly IProductRepo _productRepo = productRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;

		public async Task<AppResult<Unit>> Handle(CreateNewPalletCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				foreach (var product in request.DTO.ProductsOnPallet)
				{
					if (!await _productRepo.IsExistProduct(product.ProductId))
						return AppResult<Unit>.Fail($"Produkt o numerze {product.ProductId} nie istnieje.", ErrorType.NotFound);
				}
				var newIdForPallet = await _palletRepo.GetNextPalletIdAsync();
				var pallet = Pallet.Create(newIdForPallet);

				var productOnPallet = new List<ProductOnPallet>();

				foreach(var product in request.DTO.ProductsOnPallet)
				{
					pallet.AddProduct(product.ProductId, product.Quantity, product.BestBefore);
				}

				//var listProducts = request.DTO.ProductsOnPallet
				//	.Select(p => _mapper.Map<ProductOnPallet>(p)).ToList()
				//	.ToList();

				
				//pallet.ApplyProductChanges(listProducts);
				//var pallet = new Pallet(newIdForPallet, DateTime.UtcNow, listProducts);
				

				_palletRepo.AddPallet(pallet);
				var location = await _locationRepo.GetLocationByIdAsync(request.RampNumber);
				pallet.AssignToWarehouse(location, request.UserId);

				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				
				return AppResult<Unit>.Success(Unit.Value,$"Dodano paletę {newIdForPallet} do stanu magazynowego, uaktualniono stan magazynowy.");
			}			
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas aktualizaowania przyjęcia");	
				//return PalletResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.", ex.Message);
				throw;
			}
		}
	}
}
