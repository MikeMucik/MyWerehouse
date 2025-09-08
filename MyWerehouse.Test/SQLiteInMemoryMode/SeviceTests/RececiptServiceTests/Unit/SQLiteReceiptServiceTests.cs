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
using Microsoft.Extensions.Options;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Application.ViewModels.ProductOnPalletModels;
using MyWerehouse.Application.ViewModels.ReceiptModels;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;
using SQLitePCL;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.RececiptServiceTests.Unit
{
	public class SQLiteReceiptServiceTests : TestBase
	{
		[Fact]
		public async Task ProperData_AddPalletToReceiptAsync_AddedToBase()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			var mockMapper = new Mock<IMapper>();
			var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
			var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
			var mockPalletMovementService = new Mock<IPalletMovementService>();
			var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();

			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }],
				UserId = "U001"
			};
			var pallet = new Pallet { Id = "Q1000" };
			mockMapper.Setup(m => m.Map<Pallet>(newPalletDto)).Returns(pallet);
			mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());
			mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());
			mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());
			mockPalletMovementService.Setup(a => a.CreateMovementAsync(It.IsAny<Pallet>(),
				It.IsAny<int>(), It.IsAny<ReasonMovement>(),
				It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				.Returns(Task.CompletedTask);
			var mockInventoryService = new Mock<IInventoryService>();
			var receiptRepo = new ReceiptRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);
			var service = new ReceiptService(
				receiptRepo, mockMapper.Object,
				DbContext, palletRepo,
				mockPalletMovementService.Object,
				mockInventoryService.Object,
				mockValidator.Object,
				mockReceiptValidator.Object
				//,mockUpdateValidator.Object
				);
			//Act
			string newPallet = await service.AddPalletToReceiptAsync(1, newPalletDto);
			//Assert
			Assert.NotNull(newPallet);
			Assert.Equal(1, await DbContext.Pallets.CountAsync());
			var savedPallet = await DbContext.Pallets.FirstAsync();
			Assert.Equal(1, savedPallet.ReceiptId);
			Assert.Equal(PalletStatus.Receiving, savedPallet.Status);
			var updatedReceipt = await DbContext.Receipts.FindAsync(1);
			Assert.NotNull(updatedReceipt);
			Assert.Equal(ReceiptStatus.InProgress, updatedReceipt.ReceiptStatus);
			mockPalletMovementService.Verify(s => s.CreateMovementAsync(It.IsAny<Pallet>(), 1, ReasonMovement.Received, "U001", PalletStatus.Receiving, null), Times.Once);
		}
		[Fact]
		//przeniesienie do kontrolera
		public async Task AddPalletToReceiptAsync_WhenMovementServiceFails_ShouldRollbackAllChanges()
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
			var initailCLient = new Client
			{
				Id = 1,
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = [address]
			};
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.Planned,
				PerformedBy = "U002"
			};
			var initailLocation = new Location
			{
				Id = 1,
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			DbContext.Clients.Add(initailCLient);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			var mockMapper = new Mock<IMapper>();
			var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
			var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
			var mockPalletMovementService = new Mock<IPalletMovementService>();
			var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();
			var mockInventory = new Mock<IInventoryService>();
			var newPalletDto = new CreatePalletReceiptDTO
			{
				ProductsOnPallet = [new() { ProductId = 1, Quantity = 10, }],
				UserId = "U001"
			};
			var pallet = new Pallet { Id = "Q1000" };
			mockMapper.Setup(m => m.Map<Pallet>(newPalletDto)).Returns(pallet);
			mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());
			mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());
			mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());
			mockPalletMovementService.Setup(s => s.CreateMovementAsync(It.IsAny<Pallet>(), It.IsAny<int>(),
				It.IsAny<ReasonMovement>(), It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				.ThrowsAsync(new Exception("Błąd zapisu ruchu - symulacja"));
			var receiptRepo = new ReceiptRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);
			var service = new ReceiptService(
				receiptRepo, mockMapper.Object,
				DbContext, palletRepo,
				mockPalletMovementService.Object, mockInventory.Object,
				mockValidator.Object,
				mockReceiptValidator.Object
				//,mockUpdateValidator.Object
				);
			//Act
			await Assert.ThrowsAsync<Exception>(() => service.AddPalletToReceiptAsync(1, newPalletDto));
			Assert.Equal(0, await DbContext.Pallets.CountAsync());
			using var arrangeContext = CreateNewContext();
			var receiptAfterFail = await arrangeContext.Receipts.FindAsync(1);
			Assert.Equal(ReceiptStatus.Planned, receiptAfterFail.ReceiptStatus);
		}

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
				ReceiptId = 1
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
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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

			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			var mapper = MapperConfig.CreateMapper();
			var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
			var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
			var mockPalletMovementService = new Mock<IPalletMovementService>();
			var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();
			var mockInventory = new Mock<IInventoryService>();
			var updatingReceipt = new ReceiptDTO
			{
				Id = 1,
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = initialPallet.Id,
						LocationId = 1,
						ReceiptId = 1,
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

			mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(new FluentValidation.Results.ValidationResult());
			mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());

			mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());

			mockPalletMovementService.Setup(a => a.CreateMovementAsync(It.IsAny<Pallet>(),
				It.IsAny<int>(), It.IsAny<ReasonMovement>(),
				It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				.Returns(Task.CompletedTask);

			var receiptRepo = new ReceiptRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);
			var service = new ReceiptService(
				receiptRepo,
				mapper,
				DbContext,
				palletRepo,
				mockPalletMovementService.Object,
				mockInventory.Object,
				mockValidator.Object,
				mockReceiptValidator.Object
				//,mockUpdateValidator.Object
				);
			//Act			
			await service.UpdateReceiptPalletsAsync(updatingReceipt, userId);

			//Assert
			var result = DbContext.Receipts.SingleOrDefault(x => x.Id == updatingReceipt.Id);
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
				ReceiptId = 1,
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
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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
			var initialCategory = new Category
			{
				Id = 1,
				Name = "name",
				IsDeleted = false
			};
			DbContext.Categories.Add(initialCategory);
			//DbContext.Products.AddRange(initialProduct, initialProduct1);
			DbContext.Products.Add(initialProduct);
			DbContext.ProductOnPallet.Add(initialProductOnPallet);
			DbContext.Pallets.Add(initialPallet);
			DbContext.Clients.AddRange(initailCLient, initailCLient1);
			DbContext.Receipts.Add(initialReceipt);
			DbContext.Locations.Add(initailLocation);
			await DbContext.SaveChangesAsync();

			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			var mapper = MapperConfig.CreateMapper();
			var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
			var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
			var mockPalletMovementService = new Mock<IPalletMovementService>();
			var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();
			var mockInventory = new Mock<IInventoryService>();
			var updatingReceipt = new ReceiptDTO
			{
				Id = 1,
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						Id = initialPallet.Id,
						LocationId = 1,
						ReceiptId = 10, //WrongData
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

			mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(new FluentValidation.Results.ValidationResult());
			mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());

			mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());

			mockPalletMovementService.Setup(a => a.CreateMovementAsync(It.IsAny<Pallet>(),
				It.IsAny<int>(), It.IsAny<ReasonMovement>(),
				It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				.Returns(Task.CompletedTask);

			var receiptRepo = new ReceiptRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);

			var service = new ReceiptService(
				receiptRepo,
				mapper,
				DbContext,
				palletRepo,
				mockPalletMovementService.Object,
				mockInventory.Object,
				mockValidator.Object,
				mockReceiptValidator.Object
				//,mockUpdateValidator.Object
				);
			//Act&Assert		
			var ex = await Assert.ThrowsAsync<InvalidDataException>(() => service.UpdateReceiptPalletsAsync(updatingReceipt, userId));
			Assert.Contains("należy do innego przyjęcia o numerze", ex.Message);

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
				ReceiptId = 1,
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
			var initialReceipt = new Receipt
			{
				Id = 1,
				ClientId = 1,
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				PerformedBy = "U002",
				ReceiptDateTime = new DateTime(2025, 6, 6),
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

			var MapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			var mapper = MapperConfig.CreateMapper();
			var mockValidator = new Mock<IValidator<CreatePalletReceiptDTO>>();
			var mockReceiptValidator = new Mock<IValidator<ReceiptDTO>>();
			var mockPalletMovementService = new Mock<IPalletMovementService>();
			var mockUpdateValidator = new Mock<IValidator<UpdatePalletDTO>>();
			var mockInventory = new Mock<IInventoryService>();
			var updatingReceipt = new ReceiptDTO
			{
				Id = 1,
				ClientId = 2,
				PerformedBy = "U002",
				ReceiptStatus = ReceiptStatus.PhysicallyCompleted,
				ReceiptDateTime = new DateTime(2025, 6, 6),
				Pallets =
				new List<UpdatePalletDTO>
				{
					new()
					{
						LocationId = 1,
						ReceiptId = 1,
						Status = PalletStatus.Receiving,
						DateReceived = DateTime.Now,
						ProductsOnPallet = new List<ProductOnPalletDTO>
						{
							new()
							{
								ProductId = 1,
								Quantity = 1,
								DateAdded = DateTime.Now,
							}
						}
					}
				}
			};
			var userId = "U100";

			mockValidator.Setup(m => m.Validate(It.IsAny<CreatePalletReceiptDTO>())).Returns(new FluentValidation.Results.ValidationResult());
			mockUpdateValidator.Setup(m => m.Validate(It.IsAny<UpdatePalletDTO>())).Returns(new FluentValidation.Results.ValidationResult());

			mockReceiptValidator.Setup(m => m.Validate(It.IsAny<ReceiptDTO>())).Returns(
				new FluentValidation.Results.ValidationResult());

			mockPalletMovementService.Setup(a => a.CreateMovementAsync(It.IsAny<Pallet>(),
				It.IsAny<int>(), It.IsAny<ReasonMovement>(),
				It.IsAny<string>(), It.IsAny<PalletStatus>(), null))
				.Returns(Task.CompletedTask);

			var receiptRepo = new ReceiptRepo(DbContext);
			var palletRepo = new PalletRepo(DbContext);

			var service = new ReceiptService(
				receiptRepo,
				mapper,
				DbContext,
				palletRepo,
				mockPalletMovementService.Object,
				mockInventory.Object,
				mockValidator.Object,
				mockReceiptValidator.Object				
				);
			//Act			
			await service.UpdateReceiptPalletsAsync(updatingReceipt, userId);

			//Assert
			var result = DbContext.Receipts.SingleOrDefault(x => x.Id == updatingReceipt.Id);
			Assert.NotNull(result);
			Assert.Equal(2, result.ClientId);
			Assert.Single(result.Pallets); // powinien zostać tylko jeden
			Assert.Equal("Q1001", result.Pallets.First().Id);
			var newPallet = DbContext.Pallets.SingleOrDefault(p => p.Id == "Q1001");
			Assert.NotNull(newPallet);
			Assert.Equal(PalletStatus.Receiving, newPallet.Status);
			Assert.Equal(1, newPallet.LocationId);
			Assert.Equal(1, newPallet.ReceiptId);
			var product = DbContext.ProductOnPallet.SingleOrDefault(p => p.Id == 2);
			Assert.NotNull(product);
			Assert.Equal("Q1001", product.PalletId);
			Assert.Equal(1, product.Quantity);
			Assert.Equal(1, product.ProductId);

			mockPalletMovementService.Verify(p =>
			p.CreateMovementAsync(It.IsAny<Pallet>(), It.IsAny<int>(), ReasonMovement.Correction, userId, PalletStatus.Receiving, null),
			Times.AtLeastOnce);
			mockPalletMovementService.Verify(p =>
			p.CreateMovementAsync(It.IsAny<Pallet>(), It.IsAny<int>(), ReasonMovement.Correction, userId, It.IsAny<PalletStatus>(), null),
			Times.AtLeastOnce);
			//Sprawdzić co się dzieje z paletą wyrzuconą z przyjęcia			
		}
	}
}