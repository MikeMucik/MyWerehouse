using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Exceptions;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;
using MyWerehouse.Infrastructure.Repositories;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Integration
{
	public class ManualPickingIntegrationTests : PickingIntegrationCommandService
	{

		[Fact]
		public async Task DoManualPicking_WithIssueId_AssignsProductAndCommits()
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
			var newToPickPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 20,
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
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			DbContext.Issues.AddRange(issue);

			var allocation2 = new Allocation
			{
				Issue = issue,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = newToPickPallet,
				IssueInitialQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				Allocation = new List<Allocation>()
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				IssueInitialQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				Allocation = new List<Allocation> { allocation2 }
			};

			allocation2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			DbContext.Issues.Add(issue);
			await DbContext.SaveChangesAsync();
			// Act
			var result = await _pickingPalletService.DoManualPickingAsync(newToPickPallet.Id, issue.Id, "user1");

			// Assert
			Assert.True(result.Success);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			var updatedPallet = await DbContext.Pallets.FindAsync(newToPickPallet.Id);
			Assert.Contains(updatedPallet.Status, new[] { PalletStatus.Archived, PalletStatus.OnHold, PalletStatus.ToPicking });
		}

		//Metoda ExecutiveManualPicking

		[Fact]
		public async Task ExecutiveManualPicking_WithIssueId_AssignsProductAndCommits()
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
			var newToPickPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 20,
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
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			DbContext.Issues.AddRange(issue);
			var allocation2 = new Allocation
			{
				Issue = issue,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = newToPickPallet,
				IssueInitialQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				Allocation = new List<Allocation>()
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				IssueInitialQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				Allocation = new List<Allocation> { allocation2 }
			};
			allocation2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			await DbContext.SaveChangesAsync();
			// Act

			var result = await _pickingPalletService.ExecuteManualPickingAsync(newToPickPallet.Id, issue.Id, "user1");

			// Assert
			Assert.True(result.Success);
			Assert.Equal("Towar dołączono do zlecenia", result.Message);

			var updatedPallet = await DbContext.Pallets.FindAsync(newToPickPallet.Id);
			Assert.Contains(updatedPallet.Status, new[] { PalletStatus.Archived, PalletStatus.OnHold, PalletStatus.ToPicking });
		}

		[Fact]
		public async Task ExecutiveManualPicking_WithIssueId_ThrowException()
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
			var newToPickPallet = new Pallet
			{
				Id = "Q1001",
				DateReceived = new DateTime(2025, 8, 8),
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 20,
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
			DbContext.Locations.AddRange(location1, locationPicking);
			DbContext.Clients.AddRange(client);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(sourcePallet1, newToPickPallet);
			//DbContext.Issues.AddRange(issue);
			var allocation2 = new Allocation
			{
				Issue = issue,
				Quantity = 10,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = newToPickPallet,
				IssueInitialQuantity = 20,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				Allocation = new List<Allocation>()
			};
			var virtualPallet2 = new VirtualPallet
			{
				Pallet = sourcePallet1,
				IssueInitialQuantity = 10,
				Location = sourcePallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
				Allocation = new List<Allocation> { allocation2 }
			};
			allocation2.VirtualPallet = virtualPallet2;
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2);
			DbContext.Issues.Remove(issue);
			await DbContext.SaveChangesAsync();
			
			// Act
			var result = await _pickingPalletService.ExecuteManualPickingAsync(newToPickPallet.Id, issue.Id, "user1");
			// Assert
			Assert.False(result.Success);
			Assert.Equal($"Zamówienie o numerze {issue.Id} nie zostało znalezione.", result.Message);
		}
	}
}
