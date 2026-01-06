using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Services;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure.Repositories;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Histories.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class PickingServicePickerIntegrationTests : PickingIntegrationCommandService
	{
		[Fact]
		public async Task DoPickingAsync_HappyPath_CreatesNewVirtualPallet()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};			
			var product = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),				
				Category = category,
				IsDeleted = false,
				CartonsPerPallet =100
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};			
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,				
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet);
			DbContext.Issues.AddRange(issue);
			var virtualPallet = new VirtualPallet
			{
				Pallet = sourcePallet,
				IssueInitialQuantity = 40,
				Location = sourcePallet.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};
			var allocation = new Allocation
			{
				Issue = issue,
				Quantity = 30,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet,
			};
			virtualPallet.Allocations = new List<Allocation> { allocation};
			DbContext.Allocations.Add(allocation);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();
			// Act
			var allocationDTO = new AllocationDTO
			{
				AllocationId = allocation.Id,
				IssueId = issue.Id,
				ProductId = product.Id,
				RequestedQuantity = allocation.Quantity,
				PickedQuantity = 30,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet.Id
			};
			var result = await _pickingPalletService.DoPickingAsync(allocationDTO, "user1");

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			var updatedAllocation = await DbContext.Allocations.FindAsync(allocation.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p=>p.ProductsOnPallet)				
				.FirstAsync(p => p.Id == sourcePallet.Id);			
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);
						
			// Assert Allocation
			Assert.NotNull(updatedAllocation);
			Assert.Equal(PickingStatus.Picked, updatedAllocation.PickingStatus);
			// Assert Source Pallet (powinno zostać 10)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(10, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			// Assert New Pallet (powinno powstać 30 sztuk na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Single(newPallet.ProductsOnPallet);
			Assert.Equal(product.Id, newPallet.ProductsOnPallet.First().ProductId);
			Assert.Equal(30, newPallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		//Cała paleta jest pobierana bo to końcówka palety
		[Fact]
		public async Task DoPickingAsync_HappyPathTakeWholePallet_CreatesNewVirtualPallet()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				//Pallets,
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet);
			DbContext.Issues.AddRange(issue);
			var virtualPallet = new VirtualPallet
			{
				Pallet = sourcePallet,
				IssueInitialQuantity = 40,
				Location = sourcePallet.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};
			var allocation = new Allocation
			{
				Issue = issue,
				Quantity = 40,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet,
			};
			virtualPallet.Allocations = new List<Allocation> { allocation };
			DbContext.Allocations.Add(allocation);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act
			var allocationDTO = new AllocationDTO
			{
				AllocationId = allocation.Id,
				IssueId = issue.Id,
				ProductId = product.Id,
				RequestedQuantity = 40,
				PickedQuantity = 40,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet.Id
			};
			var result = await _pickingPalletService.DoPickingAsync(allocationDTO, "user1");

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			var updatedAllocation = await DbContext.Allocations.FindAsync(allocation.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert Allocation
			Assert.NotNull(updatedAllocation);
			Assert.Equal(PickingStatus.Picked, updatedAllocation.PickingStatus);
			// Assert Source Pallet (powinno zostać 0)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(0, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Archived , updatedSourcePallet.Status);
			// Assert New Pallet (powinno powstać 40 sztuk na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Single(newPallet.ProductsOnPallet);
			Assert.Equal(product.Id, newPallet.ProductsOnPallet.First().ProductId);
			Assert.Equal(40, newPallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheSameProductToExistPickingPallet_AddToVirrtualPallet()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 100,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var oldPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Picking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product,
						Quantity = 10,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				Pallets = [oldPallet],
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product);
			DbContext.Pallets.AddRange(sourcePallet1
				, oldPallet
				);
			DbContext.Issues.AddRange(issue);
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				IssueInitialQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};
			var allocation1 = new Allocation
			{
				Issue = issue,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet1,
			};
			virtualPallet1.Allocations = new List<Allocation> { allocation1 };			
			DbContext.Allocations.AddRange(allocation1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var allocationDTO = new AllocationDTO
			{
				AllocationId = allocation1.Id,
				IssueId = issue.Id,
				ProductId = product.Id,
				RequestedQuantity = allocation1.Quantity,
				PickedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id
			};
			var result = await _pickingPalletService.DoPickingAsync(allocationDTO, "user1");

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			var updatedAllocation = await DbContext.Allocations.FindAsync(allocation1.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet1.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert Allocation
			Assert.NotNull(updatedAllocation);
			Assert.Equal(PickingStatus.Picked, updatedAllocation.PickingStatus);
			
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(90, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			
			Assert.NotNull(newPallet);
			Assert.Single(newPallet.ProductsOnPallet);
			Assert.Equal(product.Id, newPallet.ProductsOnPallet.First().ProductId);
			Assert.Equal(20, newPallet.ProductsOnPallet.First().Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheAnotherProductToExistPickingPallet_AddToVirtualPallet()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "777",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var oldPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Picking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 20,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				Pallets = [oldPallet],
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1
				, oldPallet
				);
			DbContext.Issues.AddRange(issue);
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				IssueInitialQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};
			var allocation1 = new Allocation
			{
				Issue = issue,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet1,
			};
			virtualPallet1.Allocations = new List<Allocation> { allocation1 };
			DbContext.Allocations.AddRange(allocation1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();

			// Act
			var allocationDTO = new AllocationDTO
			{
				AllocationId = allocation1.Id,
				IssueId = issue.Id,
				ProductId = product2.Id,
				RequestedQuantity = allocation1.Quantity,
				PickedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id
			};
			var result = await _pickingPalletService.DoPickingAsync(allocationDTO, "user1");

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			var updatedAllocation = await DbContext.Allocations.FindAsync(allocation1.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet1.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert Allocation
			Assert.NotNull(updatedAllocation);
			Assert.Equal(PickingStatus.Picked, updatedAllocation.PickingStatus);
			// Assert Source Pallet (powinno zostać 90)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(90, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			// Assert New Pallet (powinno powstać 10 sztuk jednego produktu i 10 sztuk drugiego produktu na palecie Picking)
			Assert.NotNull(newPallet);			
			Assert.Equal(2, newPallet.ProductsOnPallet.Count);			
			Assert.Equal(20, newPallet.ProductsOnPallet.First(p=>p.ProductId == product1.Id).Quantity);
			Assert.Equal(10, newPallet.ProductsOnPallet.First(p=>p.ProductId == product2.Id).Quantity);			
			Assert.Equal(PalletStatus.Picking, newPallet.Status);
		}
		[Fact]
		public async Task DoPickingAsync_HappyPathAddTheAnotherProductToExistPickingPalletWithHistory_AddToVirtualPallet()
		{
			// Arrange
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "777",
				AddedItemAd = new DateTime(2025, 1, 1),
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 100
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var locationPicking = new Location
			{
				Id = 100100,
				Aisle = 10,
				Bay = 1,
				Height = 1,
				Position = 1
			};
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
			var client = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address],
				IsDeleted = false,
			};
			var sourcePallet1 = new Pallet
			{
				Id = "Q1000",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var oldPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.Picking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product1,
						Quantity = 20,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var issue = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.UtcNow,
				Pallets = [oldPallet],
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			DbContext.Addresses.Add(address);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, oldPallet);
			DbContext.Issues.AddRange(issue);
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				IssueInitialQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};
			var allocation1 = new Allocation
			{
				Issue = issue,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet1,
			};
			virtualPallet1.Allocations = new List<Allocation> { allocation1 };
			DbContext.Allocations.AddRange(allocation1);
			DbContext.VirtualPallets.AddRange(virtualPallet1);
			await DbContext.SaveChangesAsync();
			
			// Act
			var allocationDTO = new AllocationDTO
			{
				AllocationId = allocation1.Id,
				IssueId = issue.Id,
				ProductId = product2.Id,
				RequestedQuantity = allocation1.Quantity,
				PickedQuantity = 10,
				PickingStatus = PickingStatus.Allocated,
				SourcePalletId = sourcePallet1.Id
			};
			var result = await _pickingPalletService.DoPickingAsync(allocationDTO, "user1");

			// Assert
			Assert.NotNull(result);
			Assert.True(result.Success);
			var updatedAllocation = await DbContext.Allocations.FindAsync(allocation1.Id);
			var updatedSourcePallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstAsync(p => p.Id == sourcePallet1.Id);
			var newPallet = await DbContext.Pallets
				.Include(p => p.ProductsOnPallet)
				.FirstOrDefaultAsync(p => p.Status == PalletStatus.Picking);

			// Assert Allocation
			Assert.NotNull(updatedAllocation);
			Assert.Equal(PickingStatus.Picked, updatedAllocation.PickingStatus);
			// Assert Source Pallet (powinno zostać 90)
			Assert.Single(updatedSourcePallet.ProductsOnPallet);
			Assert.Equal(90, updatedSourcePallet.ProductsOnPallet.First().Quantity);
			// Assert New Pallet (powinno powstać 10 sztuk jednego produktu i 10 sztuk drugiego produktu na palecie Picking)
			Assert.NotNull(newPallet);
			Assert.Equal(2, newPallet.ProductsOnPallet.Count);
			Assert.Equal(20, newPallet.ProductsOnPallet.First(p => p.ProductId == product1.Id).Quantity);
			Assert.Equal(10, newPallet.ProductsOnPallet.First(p => p.ProductId == product2.Id).Quantity);
			Assert.Equal(PalletStatus.Picking, newPallet.Status);

			// Assert Pallet Movements (historia zmian)
			var movements = await DbContext.PalletMovements
				.Where(m => m.PalletId == sourcePallet1.Id || m.PalletId == newPallet.Id)
				.ToListAsync();

			// powinny być 2 wpisy: jeden dla źródłowej palety, jeden dla kompletacyjnej
			Assert.Equal(2, movements.Count);

			// źródłowa paleta (powinna mieć ruch typu ToPicking)
			var sourceMovement = movements.FirstOrDefault(m => m.PalletId == sourcePallet1.Id);
			Assert.NotNull(sourceMovement);
			Assert.Equal(ReasonMovement.Picking, sourceMovement.Reason);
			Assert.Equal(PalletStatus.ToPicking, sourceMovement.PalletStatus);

			// paleta kompletacyjna (również powinna mieć ruch typu Picking)
			var newPalletMovement = movements.FirstOrDefault(m => m.PalletId == newPallet.Id);
			Assert.NotNull(newPalletMovement);
			Assert.Equal(ReasonMovement.Picking, newPalletMovement.Reason);
			Assert.Equal(PalletStatus.Picking, newPalletMovement.PalletStatus);

		}
	}
}
