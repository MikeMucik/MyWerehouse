using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Application.Receipts.Commands.UpdateReceipt;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Unit
{
	public class ReceiptServiceTests : TestBase
	{
		//[Fact]
		//public async Task ProperData_AddPalletToReceiptAsync_AddedToBase()
		//{
		//	//Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var initailCLient = new Client
		//	{
		//		Id = 1,
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany",
		//		Addresses = [address]
		//	};
		//	var initialReceipt = new Receipt
		//	{
		//		Id = 1,
		//		ClientId = 1,
		//		ReceiptStatus = ReceiptStatus.Planned,
		//		PerformedBy = "U002"
		//	};
		//	var initailLocation = new Location
		//	{
		//		Id = 1,
		//		Aisle = 1,
		//		Bay = 1,
		//		Height = 1,
		//		Position = 1
		//	};
		//	DbContext.Clients.Add(initailCLient);
		//	DbContext.Receipts.Add(initialReceipt);
		//	DbContext.Locations.Add(initailLocation);
		//	await DbContext.SaveChangesAsync();

		//	var mockMapper = new Mock<IMapper>();
		//	var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
		//	var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
		//	var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();

		//	var newPalletDto = new CreatePalletReceiptDTO
		//	{
		//		ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }],
		//		UserId = "U001"
		//	};
		//	var pallet = new Pallet { Id = "Q1000" };
		//	mockMapper.Setup(m => m.Map<Pallet>(newPalletDto)).Returns(pallet);
		//	mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(
		//		new FluentValidation.Results.ValidationResult());
		//	mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(
		//		new FluentValidation.Results.ValidationResult());
		//	mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());

		//	var receiptRepo = new ReceiptRepo(DbContext);
		//	var palletRepo = new PalletRepo(DbContext);
		//	var receiptNewValidator = new CreateReceiptPlanDTOValidation();
		//	var service = new ReceiptService(Mediator,
		//		receiptRepo, mockMapper.Object,
		//		DbContext, palletRepo,
		//		mockValidator.Object,
		//		mockReceiptValidator.Object,
		//		receiptNewValidator);
		//	//Act
		//	var newPallet = await service.AddPalletToReceiptAsync(1, newPalletDto);
		//	//Assert
		//	Assert.NotNull(newPallet);
		//	Assert.Equal(1, await DbContext.Pallets.CountAsync());
		//	var savedPallet = await DbContext.Pallets.FirstAsync();
		//	Assert.Equal(1, savedPallet.ReceiptId);
		//	Assert.Equal(PalletStatus.Receiving, savedPallet.Status);
		//	var updatedReceipt = await DbContext.Receipts.FindAsync(1);
		//	Assert.NotNull(updatedReceipt);
		//	Assert.Equal(ReceiptStatus.InProgress, updatedReceipt.ReceiptStatus);
		//}
		//[Fact]
		////przeniesienie do kontrolera
		//public async Task AddPalletToReceiptAsync_WhenPalletServiceFails_ShouldRollbackAllChanges()
		//{
		//	//Arrange
		//	var address = new Address
		//	{
		//		City = "Warsaw",
		//		Country = "Poland",
		//		PostalCode = "00-999",
		//		StreetName = "Wiejska",
		//		Phone = 4444444,
		//		Region = "Mazowieckie",
		//		StreetNumber = "23/3"
		//	};
		//	var initailCLient = new Client
		//	{
		//		Id = 1,
		//		Name = "TestCompany",
		//		Email = "123@op.pl",
		//		Description = "Description",
		//		FullName = "FullNameCompany",
		//		Addresses = [address]
		//	};
		//	var initialReceipt = new Receipt
		//	{
		//		Id = 1,
		//		ClientId = 1,
		//		ReceiptStatus = ReceiptStatus.Planned,
		//		PerformedBy = "U002"
		//	};
		//	var initailLocation = new Location
		//	{
		//		Id = 1,
		//		Aisle = 1,
		//		Bay = 1,
		//		Height = 1,
		//		Position = 1
		//	};
		//	DbContext.Clients.Add(initailCLient);
		//	DbContext.Receipts.Add(initialReceipt);
		//	DbContext.Locations.Add(initailLocation);
		//	await DbContext.SaveChangesAsync();

		//	//var mockMapper = new Mock<IMapper>();
		//	var newPalletDto = new CreatePalletReceiptDTO
		//	{
		//		ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }],
		//		UserId = "U001"
		//	};
		//	var pallet = new Pallet { Id = "Q1000" };
		//	//mockMapper.Setup(m => m.Map<Pallet>(newPalletDto)).Returns(pallet);

		//	//var mockPalletRepo = new Mock<IPalletRepo>();
		//	//mockPalletRepo
		//	//	.Setup(r => r.AddPallet(It.IsAny<Pallet>()))
		//	//	.Throws(new Exception("DB error"));
		//	//var receiptRepo = new ReceiptRepo(DbContext);

		//	var service = new ReceiptService(Mediator
		//		//,
		//	//	receiptRepo, mockMapper.Object,
		//	//	DbContext, mockPalletRepo.Object
		//		);
		//	//Act
		//	var result = await service.AddPalletToReceiptAsync(1, newPalletDto);
		//	Assert.Contains("Wystąpił nieoczekiwany błąd podczas operacji.", result.Message);
		//	Assert.Equal(0, await DbContext.Pallets.CountAsync());
		//	using var arrangeContext = CreateNewContext();
		//	var receiptAfterFail = await arrangeContext.Receipts.FindAsync(1);
		//	Assert.Equal(ReceiptStatus.Planned, receiptAfterFail.ReceiptStatus);
		//}

		[Fact]
		public async Task ProperDataOnlyUpdatePallet_UpdatePalletToReceiptAsync_AddedToBase()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address1 = new Address
			{
				City = "1111Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailCLient1 = new Client
			{
				Id = 2,
				Name = "222TestCompany",
				Email = "222123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address1]
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				LocationId = 1,
				Status = PalletStatus.Available,
				//ReceiptId = 1
			};
			var initialProductOnPallet = new ProductOnPallet
			{
				Id = 1,
				PalletId = "Q1000",
				ProductId = 10,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets = [initialPallet]
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialProduct = new Product
			{
				Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Id = 1,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.ProductOnPallet.Add(initialProductOnPallet);
			DbContext.Pallets.Add(initialPallet);
			DbContext.Clients.AddRange(initailCLient, initailCLient1);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			//var MapperConfig = new MapperConfiguration(cfg =>
			//{
			//	cfg.AddProfile<MappingProfile>();
			//});
			//var mapper = MapperConfig.CreateMapper();
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber = initialReceipt.ReceiptNumber,
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber =1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = initialPallet.Id,
						LocationId = 1,
						ReceiptId = initialReceipt.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								Id = initialProductOnPallet.Id,
								PalletId = initialPallet.Id,
								ProductId = 1,
								Quantity = 1,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";		
			//Act						
			await Mediator.Send(new UpdateReceiptCommand( updatingReceipt, userId));
			//Assert
			var result = DbContext.Receipts.SingleOrDefault(x => x.Id == updatingReceipt.ReceiptId);
			Assert.NotNull(result);
			Assert.Equal(2, result.ClientId);
			var pallet = result.Pallets;
			var product = pallet.First();
			Assert.Equal(1, product.ProductsOnPallet.First(x => x.ProductId == 1).Quantity);
		}
		[Fact]
		public async Task NonProperDataWrongReceiptId_UpdatePalletToReceiptAsync_ThrowException()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address1 = new Address
			{
				City = "1111Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailCLient = new Client
			{
				//Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailCLient1 = new Client
			{
				//Id = 2,
				Name = "222TestCompany",
				Email = "222123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address1]
			};
			var initialProduct = new Product
			{
				//Id = 10,
				Name = "Test",
				SKU = "666666",
				CategoryId = 1,
				IsDeleted = false,
			};
			var initialCategory = new Category
			{
				//Id = 1,
				Name = "name",
				IsDeleted = false
			};
			var initailLocation = new Location
			{
				//Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialReceipt = new Receipt
			{
				Id= Guid.NewGuid(),
				ReceiptNumber = 1,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				//Pallets = [initialPallet]
			};
			//var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt1 = new Receipt
			{
				Id = Guid.NewGuid(),
				ReceiptNumber = 2,
				Client = initailCLient1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
				//Pallets = [initialPallet]
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
			};
			var initialProductOnPallet = new ProductOnPallet
			{
				//Id = 1,
				PalletId = "Q1000",
				Product = initialProduct,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var pallet2 = new Pallet
			{
				Id = "Q3000",
				Location = initailLocation,
				Receipt = initialReceipt1, //WrongData
				Status = PalletStatus.Receiving,
				DateReceived = DateTime.Now,
				ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet
							{
								//Id = initialProductOnPallet.Id,
								//PalletId = initialPallet.Id,
								Product = initialProduct,
								Quantity = 1,
								DateAdded = DateTime.Now,
								BestBefore = new DateOnly(2027, 3, 3)
							}
						}
			};
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(initialProduct);
			DbContext.ProductOnPallet.Add(initialProductOnPallet);
			DbContext.Pallets.AddRange(initialPallet, pallet2);
			DbContext.Clients.AddRange(initailCLient, initailCLient1);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();
			var receiptId9 = Guid.Parse("99111111-1111-1111-1111-111111111111");
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber =1,
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber = 1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q3000",
						LocationId = 1,
						ReceiptId = receiptId9, //WrongData
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								Id = initialProductOnPallet.Id,
								PalletId = initialPallet.Id,
								ProductId = 1,
								Quantity = 1,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";		
			//Act&Assert			
			var result = await Mediator.Send(new UpdateReceiptCommand(updatingReceipt,userId));
			Assert.NotNull(result);
			Assert.False(result.IsSuccess);
			Assert.Contains("Paleta o numerze Q3000 należy do innego przyjęcia.", result.Error);

		}
		[Fact]
		public async Task ProperDataOneAddedOneRemoveOnePalletsAndClient_UpdatePalletToReceiptAsync_AddedToBase()
		{
			//Arrange
			var address = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address1 = new Address
			{
				City = "1111Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var initailCLient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initailCLient1 = new Client
			{
				Name = "222TestCompany",
				Email = "222123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address1]
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var initialProduct = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
			};
			var initialProduct1 = new Product
			{
				Name = "Test",
				SKU = "666666",
				Category = initialCategory,
				IsDeleted = false,
			};
			var initailLocation = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var receiptId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
			var initialReceipt = new Receipt
			{
				Id = receiptId1,
				ReceiptNumber = 1,
				Client = initailCLient,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
			};
			var initialPallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
			};
			var initialProductOnPallet = new ProductOnPallet
			{
				Pallet = initialPallet,
				Product = initialProduct,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};
			var initialPallet1 = new Pallet
			{
				Id = "Q1001",
				DateReceived = DateTime.Now,
				Location = initailLocation,
				Status = PalletStatus.Available,
				Receipt = initialReceipt,
			};
			var initialProductOnPallet1 = new ProductOnPallet
			{
				Pallet = initialPallet1,
				Product = initialProduct1,
				Quantity = 100,
				DateAdded = DateTime.Now,
				BestBefore = new DateOnly(2027, 3, 3)
			};

			initialPallet.ProductsOnPallet = new List<ProductOnPallet> { initialProductOnPallet };
			initialPallet1.ProductsOnPallet = new List<ProductOnPallet> { initialProductOnPallet1 };
			initialReceipt.Pallets = new List<Pallet> { initialPallet, initialPallet1 };

			initialProductOnPallet.PalletId = initialPallet.Id;
			initialProductOnPallet.ProductId = initialProduct.Id;
			initialProductOnPallet1.PalletId = initialPallet1.Id;
			initialProductOnPallet1.ProductId = initialProduct.Id;
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Clients.AddRange(initailCLient, initailCLient1);
			DbContext.Locations.Add(initailLocation);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
			DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);

			await DbContext.SaveChangesAsync();			
			var updatingReceipt = new ReceiptDTO
			{
				ReceiptId = initialReceipt.Id,
				ReceiptNumber= initialReceipt.ReceiptNumber,
				ClientId = initailCLient1.Id,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				RampNumber =1,
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = "Q1001",
						LocationId = initailLocation.Id,
						ReceiptId = initialReceipt.Id,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								Id = initialProductOnPallet1.Id,
								ProductId = initialProduct1.Id,
								Quantity = 1,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";
			//Act						
			await Mediator.Send(new UpdateReceiptCommand(updatingReceipt, userId));
			//Assert

			var result = DbContext.Receipts.Include(p => p.Pallets).ThenInclude(pp => pp.ProductsOnPallet).SingleOrDefault(x => x.Id == updatingReceipt.ReceiptId);
			Assert.NotNull(result);
			Assert.Equal(2, result.ClientId);
			Assert.Single(result.Pallets); // powinien zostać tylko jeden
			Assert.Equal("Q1001", result.Pallets.First().Id);
			await using var freshContext = CreateNewContext();
			var newPallet = DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefault(p => p.Id == "Q1001");
			Assert.NotNull(newPallet);
			Assert.Equal(PalletStatus.Receiving, newPallet.Status);
			Assert.Equal(1, newPallet.LocationId);
			Assert.Equal(initialReceipt.Id, newPallet.ReceiptId);
			//var product = DbContext.ProductOnPallet.FirstOrDefault(p => p.Id == 2);

			var productOnPalletChanged = newPallet.ProductsOnPallet.First();
			//var productOnPalletChanged = DbContext.ProductOnPallet.FirstOrDefault(x => x.PalletId == "Q1001");
			var product = DbContext.ProductOnPallet.FirstOrDefault(p => p.Id == initialProductOnPallet1.Id);
			Assert.NotNull(product);
			Assert.Equal("Q1001", product.PalletId);
			Assert.Equal(1, product.Quantity);
			Assert.Equal(initialProduct1.Id, product.ProductId);			
			//Sprawdzić co się dzieje z paletą wyrzuconą z przyjęcia			
		}
//		[Fact]
//		public async Task ProperDataOneAddedOneRemoveOnePalletsAndClient_UpdatePalletToReceiptAsync_AddedToBase_MockedMapper()
//		{
//			//Arrange
//			var address = new Address
//			{
//				City = "Warsaw",
//				Country = "Poland",
//				PostalCode = "00-999",
//				StreetName = "Wiejska",
//				Phone = 4444444,
//				Region = "Mazowieckie",
//				StreetNumber = "23/3"
//			};
//			var address1 = new Address
//			{
//				City = "1111Warsaw",
//				Country = "Poland",
//				PostalCode = "00-999",
//				StreetName = "Wiejska",
//				Phone = 4444444,
//				Region = "Mazowieckie",
//				StreetNumber = "23/3"
//			};
//			var initailCLient = new Client
//			{
//				Name = "TestCompany",
//				Email = "123@op.pl",
//				Description = "Description",
//				FullName = "FullNameCompany",
//				Addresses = [address]
//			};
//			var initailCLient1 = new Client
//			{
//				Name = "222TestCompany",
//				Email = "222123@op.pl",
//				Description = "Description",
//				FullName = "FullNameCompany",
//				Addresses = [address1]
//			};
//			var initialCategory = new Category { Name = "name", IsDeleted = false };
//			var initialProduct = new Product
//			{
//				Name = "Test",
//				SKU = "666666",
//				Category = initialCategory,
//				IsDeleted = false
//			};
//			var initialProduct1 = new Product
//			{
//				Name = "Test",
//				SKU = "666666",
//				Category = initialCategory,
//				IsDeleted = false
//			};
//			var initailLocation = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
//			var initialReceipt = new Receipt
//			{
//				Client = initailCLient,
//				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
//				PerformedBy = "U002",
//				ReceiptDateTime = new DateTime(2025, 6, 6)
//			};
//			var initialPallet = new Pallet
//			{
//				Id = "Q1000",
//				DateReceived = DateTime.Now,
//				Location = initailLocation,
//				Status = PalletStatus.Available,
//				Receipt = initialReceipt
//			};
//			var initialProductOnPallet = new ProductOnPallet
//			{
//				Pallet = initialPallet,
//				Product = initialProduct,
//				Quantity = 100,
//				DateAdded = DateTime.Now,
//				BestBefore = new DateOnly(2027, 3, 3)
//			};
//			var initialPallet1 = new Pallet
//			{
//				Id = "Q1001",
//				DateReceived = DateTime.Now,
//				Location = initailLocation,
//				Status = PalletStatus.Available,
//				Receipt = initialReceipt
//			};
//			var initialProductOnPallet1 = new ProductOnPallet
//			{
//				Pallet = initialPallet1,
//				Product = initialProduct1,
//				Quantity = 100,
//				DateAdded = DateTime.Now,
//				BestBefore = new DateOnly(2027, 3, 3)
//			};

//			initialPallet.ProductsOnPallet = new List<ProductOnPallet> { initialProductOnPallet };
//			initialPallet1.ProductsOnPallet = new List<ProductOnPallet> { initialProductOnPallet1 };
//			initialReceipt.Pallets = new List<Pallet> { initialPallet, initialPallet1 };

//			initialProductOnPallet.PalletId = initialPallet.Id;
//			initialProductOnPallet.ProductId = initialProduct.Id;
//			initialProductOnPallet1.PalletId = initialPallet1.Id;
//			initialProductOnPallet1.ProductId = initialProduct.Id;

//			DbContext.Categories.Add(initialCategory);
//			DbContext.Products.AddRange(initialProduct, initialProduct1);
//			DbContext.Clients.AddRange(initailCLient, initailCLient1);
//			DbContext.Locations.Add(initailLocation);
//			DbContext.Receipts.Add(initialReceipt);
//			DbContext.Pallets.AddRange(initialPallet, initialPallet1);
//			DbContext.ProductOnPallet.AddRange(initialProductOnPallet, initialProductOnPallet1);

//			await DbContext.SaveChangesAsync();

//			// 🧩 Mockowany mapper – zwraca po prostu ten sam obiekt, nie zmienia właściwości
//			var mockMapper = new Mock<IMapper>();
//			mockMapper.Setup(m => m.Map<ReceiptDTO, Receipt>(It.IsAny<ReceiptDTO>(), It.IsAny<Receipt>()))
//					  .Returns((ReceiptDTO src, Receipt dest) => dest);

//			// Pozostałe mocki
//			var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
//			var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
//			var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();
//			var mockLocationRepo = new Mock<ILocationRepo>();

//			var updatingReceipt = new ReceiptDTO
//			{
//				Id = initialReceipt.Id,
//				ClientId = initailCLient1.Id,
//				PerformedBy = "U002",
//				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
//				ReceiptDateTime = new DateTime(2025, 6, 6),
//				Pallets =
//				[
//					new()
//			{
//				Id = "Q1001",
//				LocationId = initailLocation.Id,
//				ReceiptId = initialReceipt.Id,
//				Status = PalletStatus.Receiving,
//				DateReceived = DateTime.Now,
//				ProductsOnPallet =
//				[
//					new()
//					{
//						Id = initialProductOnPallet1.Id,
//						ProductId = initialProduct1.Id,
//						Quantity = 1,
//						DateAdded = DateTime.Now
//					}
//				]
//			}
//				]
//			};

//			var userId = "U100";
//			mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(new FluentValidation.Results.ValidationResult());
//			mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());
//			mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(new FluentValidation.Results.ValidationResult());

//			var receiptRepo = new ReceiptRepo(DbContext);
//			var palletRepo = new PalletRepo(DbContext);
//			var receiptNewValidator = new CreateReceiptPlanDTOValidation();
//			var service = new ReceiptService(Mediator,
//				receiptRepo,
//				mockMapper.Object, // 🔥 zamockowany mapper
//				DbContext,
//				palletRepo,
//				mockValidator.Object,
//				mockReceiptValidator.Object,
//				receiptNewValidator
//			);

//			//Act
//			await service.UpdateReceiptPalletsAsync(updatingReceipt, userId);

//			//Assert
//			var result = DbContext.Receipts
//				.Include(p => p.Pallets)
//				.ThenInclude(pp => pp.ProductsOnPallet)
//				.SingleOrDefault(x => x.Id == updatingReceipt.Id);

//			Assert.NotNull(result);
//			Assert.Equal(initailCLient1.Id, result.ClientId);
//			Assert.Single(result.Pallets); // tylko jedna paleta
//			Assert.Equal("Q1001", result.Pallets.First().Id);

//			service.Verify(r => r.UpdateAsync(
//	It.Is<Receipt>(r =>
//		r.Pallets.Any(p =>
//			p.ProductsOnPallet.Any(prod => prod.Quantity == 50)
//		)
//	)
//), Times.Once);
//			////potrzebny nowy Context
//			//await using var freshContext = CreateNewContext();

//			//var newPallet = freshContext.Pallets
//			//	.Include(p => p.ProductsOnPallet)
//			//	.FirstOrDefault(p => p.Id == "Q1001");

//			//Assert.NotNull(newPallet);
//			//Assert.Equal(PalletStatus.Receiving, newPallet.Status);
//			//Assert.Equal(initailLocation.Id, newPallet.LocationId);
//			//Assert.Equal(initialReceipt.Id, newPallet.ReceiptId);

//			//var product = DbContext.ProductOnPallet.FirstOrDefault(p => p.PalletId == "Q1001");
//			//Assert.NotNull(product);
//			//Assert.Equal("Q1001", product.PalletId);
//			//Assert.Equal(1, product.Quantity);
//			//Assert.Equal(initialProduct1.Id, product.ProductId);
//		}
	}
}