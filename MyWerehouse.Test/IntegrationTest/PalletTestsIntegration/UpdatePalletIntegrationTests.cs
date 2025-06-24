using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.IntegrationTest.PalletTestsIntegration
{
	public class UpdatePalletIntegrationTests : PalletIntergrationCommand
	{
		[Fact]
		public void ProperData_UpdatePallet_ChangeData()
		{
			//Arange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var product1 = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded =	new DateTime(2025,4,4,8,8,8),
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var product2 = new ProductOnPallet
			{
				Id = 2,
				PalletId = "Q1000",
				ProductId = 20,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 4)
			};
			var updatingPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet =new[] { product1, product2 }
			};
			arrangeContext.Pallets.Add(updatingPallet);
			arrangeContext.ProductOnPallet.AddRange(product1, product2);
			arrangeContext.SaveChanges();
			//Act
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1000",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = [ ( new ProductOnPalletDTO
				{
					Id = product1.Id,
					PalletId = "Q1000",
					ProductId = 10,
					Quantity = 100,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 3)
				}),(new ProductOnPalletDTO
				{
					Id = product2.Id,
					PalletId = "Q1000",
					ProductId = 20,
					Quantity = 300,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 3, 4) })
					,
				(new ProductOnPalletDTO
				{
					//Id = 300,
					PalletId = "Q1000",
					ProductId = 30,
					Quantity = 200,
					DateAdded = DateTime.Now,
					BestBefore = new DateOnly(2027, 5, 4) })
					]
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _palletRepo = new PalletRepo(actContext);
				var _productOnPalletValidator = new ProductOnPalletDTOValidation();				
				var _palletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
				var _palletService = new PalletService(_palletRepo,_mapper, _palletValidator);
				_palletService.UpdatePallet(updatedPallet);
			}
			//Assert
			using(var assertContext = new WerehouseDbContext(_contextOptions))
			{
				var result = assertContext.Pallets
					.Include(p => p.ProductsOnPallet)
					.Single(x => x.Id == updatingPallet.Id);
				Assert.NotNull(result);
				Assert.Equal(updatedPallet.Status, result.Status);
				Assert.Equal(updatedPallet.ProductsOnPallet.Count, result.ProductsOnPallet.Count);
				var updatedQty = updatedPallet.ProductsOnPallet.First(x=>x.Id ==product2.Id).Quantity;
				var resultQty = result.ProductsOnPallet.First( x => x.Id ==product2.Id).Quantity;
				Assert.Equal(updatedQty, resultQty);
			}
		}
		[Fact]
		public void NonProductData_UpdatePallet_ChangeData()
		{
			//Arange
			using var arrangeContext = new WerehouseDbContext(_contextOptions);
			var product1 = new ProductOnPallet
			{
				Id = 10,
				PalletId = "Q1001",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var product2 = new ProductOnPallet
			{
				Id = 20,
				PalletId = "Q1001",
				ProductId = 20,
				Quantity = 200,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 4)
			};
			var updatingPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				LocationId = 1,
				Status = PalletStatus.Available,
				ProductsOnPallet = new[] { product1, product2 }
			};
			arrangeContext.Pallets.Add(updatingPallet);
			arrangeContext.ProductOnPallet.AddRange(product1, product2);
			arrangeContext.SaveChanges();
			//Act&Assert
			var updatedPallet = new UpdatePalletDTO
			{
				Id = "Q1001",
				DateReceived = new DateTime(2020, 1, 1, 0, 0, 0),
				
			};
			using (var actContext = new WerehouseDbContext(_contextOptions))
			{
				var _palletRepo = new PalletRepo(actContext);
				var _productOnPalletValidator = new ProductOnPalletDTOValidation();				
				var _palletValidator = new UpdatePalletDTOValidation(_productOnPalletValidator);
				var _palletService = new PalletService(_palletRepo, _mapper, _palletValidator);
				var ex = Assert.Throws<ValidationException>(() => _palletService.UpdatePallet(updatedPallet));
				Assert.Contains("Paleta musi zawierać towar/y", ex.Message);
			}			
		}
	}
}
