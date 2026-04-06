using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;


namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueCreateNewIssueIntegrationTests : TestBase
	{
		//HappyPath
		[Fact]
		public async Task CreatenewIssue_AssignsFullPalletsAndAllocatesRest_HappyPath()
		{
			// Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};

			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
					
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Available, null, null);
			pallet2.AddProductForTests(product.Id, 9, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));

			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();

			// Act			
			var issueItem = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User11",
				Items = [new IssueItemDTO
				{
					ProductId = product.Id,
					Quantity = 26, // 2 pełne palety + 5 do pickingu
					BestBefore = new DateOnly(2025, 10, 10),
				}]
			};
			var resultForIssue = await Mediator.Send(new CreateNewIssueCommand(issueItem, DateTime.UtcNow.AddDays(2)));

			// Assert
			var result = resultForIssue.Result.First();
			Assert.True(result.Success);
			Assert.Contains($"Towar {product.Id} został dołączony do zlecenia.", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			var issue = await DbContext.Issues.FirstOrDefaultAsync();
			Assert.NotNull(issue);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			// Picking pallet
			var palletToPicking = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P2");
			var pickingPallet = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).SingleOrDefault();
			Assert.NotNull(palletToPicking);
			Assert.NotNull(pickingPallet);
			Assert.Equal(6, pickingPallet.PickingTasks.First().RequestedQuantity);
			Assert.Equal(3, pickingPallet.RemainingQuantity);
			Assert.Equal(pallet2.Id, pickingPallet.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking.Status);
			Assert.Equal(issue.Id, pickingPallet.PickingTasks.First().IssueId);
			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			Assert.Equal(3, movements.Count);
			Assert.Contains(movements, m => m.PalletNumber == "P1");
			Assert.Contains(movements, m => m.PalletNumber == "P2");
			Assert.Contains(movements, m => m.PalletNumber == "P3");
		}
		[Fact]
		public async Task CreatenewIssue_AssignsFullAndRestPalletsAndAllocatesRestFromVirtualPallet_HappyPath()
		{
			// Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location4 = new Location
			{
				Aisle = 4,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location5 = new Location
			{
				Aisle = 5,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);
			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.ToPicking, null, null);
			pallet2.AddProductForTests(product.Id, 2, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			
			var pallet4 = Pallet.CreateForTests("P4", new DateTime(2025, 3, 3), 4, PalletStatus.Available, null, null);
			pallet4.AddProductForTests(product.Id, 3, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			
			var pallet5 = Pallet.CreateForTests("P5", new DateTime(2025, 3, 3), 5, PalletStatus.Available, null, null);
			pallet5.AddProductForTests(product.Id, 2, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3, location4, location5);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4, pallet5);
			await DbContext.SaveChangesAsync();
			var virtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet2.Id, 2, location2.Id, DateTime.UtcNow.AddDays(-7));
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User11",
				Items = [new IssueItemDTO
				{
					ProductId = product.Id,
					Quantity = 26, // 2 pełne palety + 5 do pickingu
					BestBefore = new DateOnly(2025, 10, 10),
				}]
			};
			var resultForIssue = await Mediator.Send(new CreateNewIssueCommand(issueItem, DateTime.UtcNow.AddDays(2)));
			// Assert
			var result = resultForIssue.Result.First();
			Assert.True(result.Success);
			Assert.Contains($"Towar {product.Id} został dołączony do zlecenia.", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			var issue = await DbContext.Issues.FirstOrDefaultAsync();
			Assert.NotNull(issue);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(2, issue.Pallets.Count); // 2 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			//Picking pallet
			var palletToPickingP2 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P2");
			var pickingPalletP2 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(x => x.PalletId == pallet2.Id);
			Assert.NotNull(palletToPickingP2);
			Assert.NotNull(pickingPalletP2);
			Assert.Equal(2, pickingPalletP2.PickingTasks.First().RequestedQuantity);
			Assert.Equal(0, pickingPalletP2.RemainingQuantity);
			Assert.Equal(pallet2.Id, pickingPalletP2.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPickingP2.Status);
			Assert.Equal(issue.Id, pickingPalletP2.PickingTasks.First().IssueId);

			var palletToPickingP4 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P4");
			var pickingPalletP4 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(x => x.PalletId == pallet4.Id);
			Assert.NotNull(palletToPickingP4);
			Assert.NotNull(pickingPalletP4);
			Assert.Equal(3, pickingPalletP4.PickingTasks.First().RequestedQuantity);//
																					//Assert.Equal(2, pickingPalletP4.PickingTasks.First().RequestedQuantity);//
			Assert.Equal(0, pickingPalletP4.RemainingQuantity);
			Assert.Equal(pallet4.Id, pickingPalletP4.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPickingP4.Status);
			Assert.Equal(issue.Id, pickingPalletP4.PickingTasks.First().IssueId);

			var palletToPickingP5 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P5");
			var pickingPalletP5 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(x => x.PalletId == pallet5.Id);
			Assert.NotNull(palletToPickingP5);
			Assert.NotNull(pickingPalletP5);
			Assert.Equal(1, pickingPalletP5.PickingTasks.First().RequestedQuantity);//
			Assert.Equal(1, pickingPalletP5.RemainingQuantity);
			Assert.Equal(pallet5.Id, pickingPalletP5.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPickingP5.Status);
			Assert.Equal(issue.Id, pickingPalletP5.PickingTasks.First().IssueId);
			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			Assert.Equal(4, movements.Count);
			Assert.Contains(movements, m => m.PalletNumber == "P1");
			Assert.Contains(movements, m => m.PalletNumber == "P5");
			Assert.Contains(movements, m => m.PalletNumber == "P3");
			Assert.Contains(movements, m => m.PalletNumber == "P4");
		}
		[Fact]
		public async Task CreateNewIssueAsync_AssignsFullPalletsAndAllocatesRest_HappyPath()
		{
			// Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location4 = new Location
			{
				Aisle = 4,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location5 = new Location
			{
				Aisle = 5,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory1 = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var initialCategory2 = new Category
			{
				Name = "name2",
				IsDeleted = false
			};
			var product1 = Product.Create("TestFull", "123", 1, 10);

			var product2 = Product.Create("TestFull", "123", 1, 10);
			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product1.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));

			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Available, null, null);
			pallet2.AddProductForTests(product1.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product1.Id, 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet4 = Pallet.CreateForTests("P4", new DateTime(2025, 3, 3), 4, PalletStatus.Available, null, null);
			pallet4.AddProductForTests(product2.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet5 = Pallet.CreateForTests("P5", new DateTime(2025, 3, 3), 5, PalletStatus.Available, null, null);
			pallet5.AddProductForTests(product2.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.AddRange(initialCategory1, initialCategory2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location1, location2, location3, location4, location5);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4, pallet5);
			await DbContext.SaveChangesAsync();
			var createIssue = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{   new IssueItemDTO
					{
						ProductId = product1.Id,
						Quantity = 26, // 2 pełne palety + 6 do pickingu
						BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
						//BestBefore = new DateOnly(2025, 10, 10)
					},  new IssueItemDTO
					{
						ProductId = product2.Id,
						Quantity = 17, // 1 pełne palety + 7 do pickingu
						BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
					}
				}
			};
			await Mediator.Send(new CreateNewIssueCommand(createIssue, DateTime.UtcNow.AddDays(7)));

			// Assert
			var issue = DbContext.Issues.First();
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(3, issue.Pallets.Count); // 3 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			// Picking pallet
			var partialPallets = DbContext.Pallets.Where(p => p.Status == PalletStatus.ToPicking).ToList();
			Assert.Equal(2, partialPallets.Count); // P3, P5

			var palletToPicking1 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P3");
			var pickingPallet1 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(p => p.PalletId == pallet3.Id);
			Assert.NotNull(palletToPicking1);
			Assert.NotNull(pickingPallet1);
			Assert.Equal(6, pickingPallet1.PickingTasks.First().RequestedQuantity);
			//Assert.Equal(6, pickingPallet1.PickingTask.FirstOrDefault(i=>i.IssueId == issue.Id).Quantity);
			Assert.Equal(4, pickingPallet1.RemainingQuantity);
			Assert.Equal(pallet3.Id, pickingPallet1.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking1.Status);
			Assert.Equal(issue.Id, pickingPallet1.PickingTasks.First().IssueId);

			var palletToPicking2 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P5");
			var pickingPallet2 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(p => p.PalletId == pallet5.Id);
			Assert.NotNull(palletToPicking2);
			Assert.NotNull(pickingPallet2);
			Assert.Equal(7, pickingPallet2.PickingTasks.First().RequestedQuantity);
			Assert.Equal(3, pickingPallet2.RemainingQuantity);
			Assert.Equal(pallet5.Id, pickingPallet2.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking2.Status);
			Assert.Equal(issue.Id, pickingPallet2.PickingTasks.First().IssueId);

			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			Assert.Equal(5, movements.Count);
			Assert.Contains(movements, m => m.PalletNumber == "P1");
			Assert.Contains(movements, m => m.PalletNumber == "P2");
			Assert.Contains(movements, m => m.PalletNumber == "P3");
			Assert.Contains(movements, m => m.PalletNumber == "P4");
			Assert.Contains(movements, m => m.PalletNumber == "P5");
		}
		[Fact]
		public async Task CreateNewIssueAsync_AssignsFullPalletsAndAllocatesRestWithPickingPalletIncludeOtherPickingTask_HappyPath()
		{
			// Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location4 = new Location
			{
				Aisle = 4,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location5 = new Location
			{
				Aisle = 5,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory1 = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var initialCategory2 = new Category
			{
				Name = "name2",
				IsDeleted = false
			};
			var product1 = Product.Create("TestFull", "123", 1, 10);

			var product2 = Product.Create("TestFull", "123", 1, 10);
			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product1.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Available, null, null);
			pallet2.AddProductForTests(product1.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.ToPicking, null, null);
			pallet3.AddProductForTests(product1.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet4 = Pallet.CreateForTests("P4", new DateTime(2025, 3, 3), 4, PalletStatus.Available, null, null);
			pallet4.AddProductForTests(product2.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet5 = Pallet.CreateForTests("P5", new DateTime(2025, 3, 3), 5, PalletStatus.Available, null, null);
			pallet5.AddProductForTests(product2.Id, 10, new DateTime(2025, 4, 4), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var issueId = Guid.NewGuid();
			
			var oldIssue = Issue.CreateForSeed(issueId, 1,1, new DateTime(2025, 8, 8),
			 new DateTime(2025, 8, 15), "TestUser", IssueStatus.New, null);
			var sourcePallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, 10, 3, new DateTime(2025, 9, 1));
			var pickingGuid = Guid.NewGuid();
			var pickingTask = PickingTask.CreateForSeed(pickingGuid, sourcePallet.Id, issueId, 2, PickingStatus.Allocated, product1.Id,
				null, null, null, 0);
			//sourcePallet.PickingTasks = new List<PickingTask> { pickingTask };
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.AddRange(initialCategory1, initialCategory2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Locations.AddRange(location1, location2, location3, location4, location5);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4, pallet5);
			DbContext.Issues.Add(oldIssue);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(sourcePallet);
			await DbContext.SaveChangesAsync();

			// Act
			var issueItem1 = new IssueItemDTO
			{
				ProductId = product1.Id,
				Quantity = 26, // 2 pełne palety + 6 do pickingu
				BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30)),
			};
			var issueItem2 = new IssueItemDTO
			{
				ProductId = product2.Id,
				Quantity = 17, // 1 pełne palety + 7 do pickingu
				BestBefore = DateOnly.FromDateTime(DateTime.Now.AddDays(30))
			};
			var createIssue = new CreateIssueDTO
			{
				//Id = Guid.NewGuid(),
				ClientId = initailClient.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					issueItem1, issueItem2
				}
			};
			var result = await Mediator.Send(new CreateNewIssueCommand(createIssue, DateTime.UtcNow.AddDays(7)));
			// Assert
			Assert.NotNull(result);
			//var issue = DbContext.Issues.FirstOrDefault(i => i.Id == createIssue.Id);
			var issue = DbContext.Issues.FirstOrDefault(i => i.IssueNumber == 2);
			Assert.Equal(IssueStatus.Pending, issue.IssueStatus);
			Assert.Equal(3, issue.Pallets.Count); // 3 pełne palety przypisane
			Assert.All(issue.Pallets, p => Assert.Equal(PalletStatus.InTransit, p.Status));
			// Picking pallet
			var partialPallets = DbContext.Pallets.Where(p => p.Status == PalletStatus.ToPicking).ToList();
			Assert.Equal(2, partialPallets.Count); // P3, P5


			var palletToPicking1 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P3");
			var pickingPallet1 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(p => p.PalletId == pallet3.Id);
			Assert.NotNull(palletToPicking1);
			Assert.NotNull(pickingPallet1);
			//Assert.Equal(6, pickingPallet1.PickingTask.First().Quantity);
			Assert.Equal(6, pickingPallet1.PickingTasks.FirstOrDefault(i => i.IssueId == issue.Id).RequestedQuantity);
			Assert.Equal(2, pickingPallet1.RemainingQuantity); //bo zarezerzowane z innego wydania
			Assert.Equal(pallet3.Id, pickingPallet1.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking1.Status);
			//Assert.Equal(issue.Id, pickingPallet1.PickingTask.FirstOrDefault(i=>i.).IssueId);

			var palletToPicking2 = DbContext.Pallets.FirstOrDefault(p => p.PalletNumber == "P5");
			var pickingPallet2 = DbContext.VirtualPallets.Include(pp => pp.PickingTasks).FirstOrDefault(p => p.PalletId == pallet5.Id);
			Assert.NotNull(palletToPicking2);
			Assert.NotNull(pickingPallet2);
			Assert.Equal(7, pickingPallet2.PickingTasks.First().RequestedQuantity);
			Assert.Equal(3, pickingPallet2.RemainingQuantity);
			Assert.Equal(pallet5.Id, pickingPallet2.PalletId);
			Assert.Equal(PalletStatus.ToPicking, palletToPicking2.Status);
			Assert.Equal(issue.Id, pickingPallet2.PickingTasks.First().IssueId);

			// Movements
			var movements = DbContext.PalletMovements.ToList();
			Assert.NotEmpty(movements);
			//Assert.Equal(5, movements.Count);
			Assert.Contains(movements, m => m.PalletNumber == "P1");
			Assert.Contains(movements, m => m.PalletNumber == "P2");
			//Assert.Contains(movements, m => m.PalletId == "P3");
			Assert.Contains(movements, m => m.PalletNumber == "P4");
			Assert.Contains(movements, m => m.PalletNumber == "P5");
		}
		//SadPath
		[Fact]
		public async Task CreatenewIssue_NotEnoughProduct_ThrowInfo()
		{
			// Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);
			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Available, null, null);
			pallet2.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
						
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product.Id, 10, new DateOnly(2026, 1, 1));
			
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);

			await DbContext.SaveChangesAsync();

			//Act
			var issueItem = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User11",
				Items = [new IssueItemDTO
				{
					ProductId = product.Id,
					Quantity = 31, // 2 pełne palety + 5 do pickingu
					BestBefore = new DateOnly(2025, 10, 10),
				}]
			};
			var resultForIssue = await Mediator.Send(new CreateNewIssueCommand(issueItem, DateTime.UtcNow.AddDays(2)));

			// Assert
			var result = resultForIssue.Result.First();
			// Assert
			Assert.False(result.Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}. Asortyment nie został dodany do zlecenia.", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(issueItem.Items.First().Quantity, result.QuantityRequest);

			var stock = 30; //Quantity P1+P2+P3
			Assert.Equal(stock, result.QuantityOnStock);
		}

		[Fact]
		public async Task CreatenewIssue_BadBestBeforeProduct_ThrowInfo()
		{
			// Arrange
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
			var initailClient = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 2,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 3,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var initialCategory = new Category
			{
				Name = "name",
				IsDeleted = false
			};
			var product = Product.Create("TestFull", "123", 1, 10);

			var pallet1 = Pallet.CreateForTests("P1", new DateTime(2025, 3, 3), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			var pallet2 = Pallet.CreateForTests("P2", new DateTime(2025, 3, 3), 2, PalletStatus.Available, null, null);
			pallet2.AddProductForTests(product.Id, 10, new DateTime(2025, 4, 4), new DateOnly(2026, 1, 1));
			var pallet3 = Pallet.CreateForTests("P3", new DateTime(2025, 3, 3), 3, PalletStatus.Available, null, null);
			pallet3.AddProduct(product.Id, 10, new DateOnly(2024, 1, 1));
			
			DbContext.Addresses.Add(address);
			DbContext.Clients.Add(initailClient);
			DbContext.Categories.Add(initialCategory);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location1, location2, location3);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3);
			await DbContext.SaveChangesAsync();

			// Act

			var issueItem = new CreateIssueDTO
			{
				ClientId = initailClient.Id,
				PerformedBy = "User11",
				Items = [new IssueItemDTO
				{
					ProductId = product.Id,
					Quantity = 25, // 2 pełne palety + 5 do pickingu
					BestBefore = new DateOnly(2025, 10, 10),
				}]
			};
			var resultForIssue = await Mediator.Send(new CreateNewIssueCommand(issueItem, DateTime.UtcNow.AddDays(2)));

			// Assert
			var result = resultForIssue.Result.First();
			// Assert
			Assert.False(result.Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}. Asortyment nie został dodany do zlecenia.", result.Message);
			Assert.Equal(product.Id, result.ProductId);
			Assert.Equal(issueItem.Items.First().Quantity, result.QuantityRequest);

			var stock = 20; //Quantity P1+P2 P3 BestBeforeWrong
			Assert.Equal(stock, result.QuantityOnStock);
		}
	}
}
