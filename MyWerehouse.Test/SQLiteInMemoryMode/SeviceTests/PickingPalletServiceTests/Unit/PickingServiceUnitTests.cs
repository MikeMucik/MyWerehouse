using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Application.PickingPallets.Queries.GetListToPicking;
using MyWerehouse.Application.PickingPallets.Queries.GetListIssueToPicking;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.PickingPalletServiceTests.Unit
{
	public class PickingServiceUnitTests : TestBase
	{
		[Fact]
		public async Task GetListToPicking_ShouldGroupByClientIssueAndProduct()
		{
			// Arrange		
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 50);			
			var product2 = Product.Create("Prod B", "667", 1, 100);			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 2
			};
			var location4 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 3
			};
			var address1 = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address2 = new Address
			{
				City = "Cracow",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "mar",
				Phone = 222222,
				Region = "Małopolskie",
				StreetNumber = "45/3"
			};
			var client1 = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address1],
				IsDeleted = false,
			};
			var client2 = new Client
			{
				Name = "Client B",
				Email = "333@wp.pl",
				Description = "des2",
				FullName = "full333",
				Addresses = [address2],
				IsDeleted = false,
			};

			var pallet1 = Pallet.CreateForTests("Q10", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product1.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet2 = Pallet.CreateForTests("Q11", new DateTime(2025, 9, 9), 2, PalletStatus.ToPicking, null, null);
			pallet2.AddProductForTests(product1.Id, 50, new DateTime(2025, 9, 9), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet3 = Pallet.CreateForTests("Q12", new DateTime(2025, 10, 10), 3, PalletStatus.ToPicking, null, null);
			pallet3.AddProductForTests(product2.Id, 70, new DateTime(2025, 10, 10), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var pallet4 = Pallet.CreateForTests("Q13", new DateTime(2025, 11, 11), 4, PalletStatus.ToPicking, null, null);
			pallet4.AddProductForTests(product2.Id, 100, new DateTime(2025, 11, 11), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			
			var issue1Id = Guid.NewGuid();
			var issue1 = Issue.CreateForSeed(issue1Id, 1, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			
			var issue2Id = Guid.NewGuid();
			var issue2 = Issue.CreateForSeed(issue2Id, 2, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			
			var issue3Id = Guid.NewGuid();
			var issue3 = Issue.CreateForSeed(issue3Id, 3, 2, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			
			DbContext.Addresses.AddRange(address1, address2);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, location3, location4);
			DbContext.Clients.AddRange(client1, client2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Issues.AddRange(issue1, issue2, issue3);		
			DbContext.SaveChanges();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet1.Id, 40, location1.Id, new DateTime(2025, 8, 12));			
			var pickingGuid11 = Guid.NewGuid();
			var a11 = PickingTask.CreateForSeed(pickingGuid11, virtualPallet1.Id, issue1Id, 10, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),0);
			var pickingGuid12 = Guid.NewGuid();
			var a12 = PickingTask.CreateForSeed(pickingGuid12, virtualPallet1.Id, issue2Id, 15, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { a11, a12 };
			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet2.Id, 50, location2.Id, new DateTime(2025, 8, 12));			
			var pickingGuid21 = Guid.NewGuid();
			var a21 = PickingTask.CreateForSeed(pickingGuid21, virtualPallet2.Id, issue1Id, 20, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			var pickingGuid22 = Guid.NewGuid();
			var a22 = PickingTask.CreateForSeed(pickingGuid22, virtualPallet2.Id, issue3Id, 25, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet2.PickingTasks = new List<PickingTask> { a21, a22 };
			var virtualPallet3 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, 50, location3.Id, new DateTime(2025, 8, 12));
			var pickingGuid31 = Guid.NewGuid();
			var a31 = PickingTask.CreateForSeed(pickingGuid31, virtualPallet3.Id, issue2Id, 15, PickingStatus.Allocated, product2.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet3.PickingTasks = new List<PickingTask> { a31 };
			var virtualPallet4 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet4.Id, 40, location4.Id, new DateTime(2025, 8, 12));
			var pickingGuid41 = Guid.NewGuid();
			var a41 = PickingTask.CreateForSeed(pickingGuid41, virtualPallet4.Id, issue1Id, 10, PickingStatus.Allocated, product2.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet4.PickingTasks = new List<PickingTask> { a41 };

			DbContext.PickingTasks.AddRange(a11, a12, a21, a22, a31, a41);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3, virtualPallet4);
			await DbContext.SaveChangesAsync();

			// Act
			
			var result = Mediator.Send(
				new GetListToPickingQuery
				(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))));

			// Assert
			result.Result.Result.Should().HaveCount(5); //Tu daje 5 bo daje alokacje każdą osobno
			//result.Equals(5, result.Result)
			// Client1, Issue1, Product1 → 10 + 20 = 30
			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue1.Id,
				IssueNumber = issue1.IssueNumber,
				ProductId = product1.Id,
				Quantity = 30
			});

			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue1.Id,
				IssueNumber = issue1.IssueNumber,
				ProductId = product2.Id,
				Quantity = 10
			});
			// Client1, Issue2, Product2 → 15
			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue2.Id,
				IssueNumber = issue2.IssueNumber,
				ProductId = product2.Id,
				Quantity = 15
			});
			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue2.Id,
				IssueNumber = issue2.IssueNumber,
				ProductId = product1.Id,
				Quantity = 15
			});
			// Client2, Issue3, Product1 → 25
			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client2.Id,
				IssueId = issue3.Id,
				IssueNumber = issue3.IssueNumber,
				ProductId = product1.Id,
				Quantity = 25
			});
		}

		//Nowa metoda inne dane wyjściowe
		[Fact]
		public async Task GetListIssueToPickingAsync_ShouldGroupByClientIssueAndProduct()
		{
			// Arrange		
			var category = new Category
			{
				Name = "Category",
				IsDeleted = false
			};
			var product1 = Product.Create("Prod A", "666", 1, 50);
			
			var product2 = Product.Create("Prod B", "667", 1, 100);
			
			var location1 = new Location
			{
				Aisle = 1,
				Bay = 1,
				Height = 1,
				Position = 1
			};
			var location2 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 1
			};
			var location3 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 2
			};
			var location4 = new Location
			{
				Aisle = 1,
				Bay = 2,
				Height = 1,
				Position = 3
			};
			var address1 = new Address
			{
				City = "Warsaw",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "Wiejska",
				Phone = 4444444,
				Region = "Mazowieckie",
				StreetNumber = "23/3"
			};
			var address2 = new Address
			{
				City = "Cracow",
				Country = "Poland",
				PostalCode = "00-999",
				StreetName = "mar",
				Phone = 222222,
				Region = "Małopolskie",
				StreetNumber = "45/3"
			};
			var client1 = new Client
			{
				Name = "Client A",
				Email = "123@wp.pl",
				Description = "des",
				FullName = "full",
				Addresses = [address1],
				IsDeleted = false,
			};
			var client2 = new Client
			{
				Name = "Client B",
				Email = "333@wp.pl",
				Description = "des2",
				FullName = "full333",
				Addresses = [address2],
				IsDeleted = false,
			};
			DbContext.Addresses.AddRange(address1, address2);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, location3, location4);
			DbContext.Clients.AddRange(client1, client2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.SaveChanges();
			var issue1Id = Guid.NewGuid();
			var issue1 = Issue.CreateForSeed(issue1Id, 101, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var issue2Id = Guid.NewGuid();
			var issue2 = Issue.CreateForSeed(issue2Id, 102, 1, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var issue3Id = Guid.NewGuid();
			var issue3 = Issue.CreateForSeed(issue3Id, 103, 2, DateTime.UtcNow,
			DateTime.UtcNow.AddDays(7), "TestUser", IssueStatus.New, null);
			var pallet1 = Pallet.CreateForTests("Q10", new DateTime(2025, 8, 8), 1, PalletStatus.Available, null, null);
			pallet1.AddProductForTests(product1.Id, 40, new DateTime(2025, 8, 8), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			var pallet2 = Pallet.CreateForTests("Q11", new DateTime(2025, 9, 9), 2, PalletStatus.ToPicking, null, null);
			pallet2.AddProductForTests(product1.Id, 50, new DateTime(2025, 9, 9), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			var pallet3 = Pallet.CreateForTests("Q12", new DateTime(2025, 10, 10), 3, PalletStatus.ToPicking, null, null);
			pallet3.AddProductForTests(product1.Id, 70, new DateTime(2025, 10, 10), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			var pallet4 = Pallet.CreateForTests("Q13", new DateTime(2025, 11, 11), 4, PalletStatus.ToPicking, null, null);
			pallet4.AddProductForTests(product2.Id, 100, new DateTime(2025, 11, 11), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)));
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Issues.AddRange(issue1, issue2, issue3);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet1.Id, 40, location1.Id, new DateTime(2025, 8, 12));
			var pickingGuid11 = Guid.NewGuid();
			var a11 = PickingTask.CreateForSeed(pickingGuid11, virtualPallet1.Id, issue1Id, 10, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			var pickingGuid12 = Guid.NewGuid();
			var a12 = PickingTask.CreateForSeed(pickingGuid12, virtualPallet1.Id, issue2Id, 15, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet1.PickingTasks = new List<PickingTask> { a11, a12 };
			var virtualPallet2 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet2.Id, 50, location2.Id, new DateTime(2025, 8, 12));
			var pickingGuid21 = Guid.NewGuid();
			var a21 = PickingTask.CreateForSeed(pickingGuid21, virtualPallet2.Id, issue1Id, 20, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			var pickingGuid22 = Guid.NewGuid();
			var a22 = PickingTask.CreateForSeed(pickingGuid22, virtualPallet2.Id, issue3Id, 25, PickingStatus.Allocated, product1.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet2.PickingTasks = new List<PickingTask> { a21, a22 };
			var virtualPallet3 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet3.Id, 50, location3.Id, new DateTime(2025, 8, 12));
			var pickingGuid31 = Guid.NewGuid();
			var a31 = PickingTask.CreateForSeed(pickingGuid31, virtualPallet3.Id, issue3Id, 15, PickingStatus.Allocated, product2.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet3.PickingTasks = new List<PickingTask> { a31 };
			var virtualPallet4 = VirtualPallet.CreateForSeed(Guid.NewGuid(), pallet4.Id, 40, location4.Id, new DateTime(2025, 8, 12));
			var pickingGuid41 = Guid.NewGuid();
			var a41 = PickingTask.CreateForSeed(pickingGuid41, virtualPallet4.Id, issue1Id, 10, PickingStatus.Allocated, product2.Id, null, null, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)), 0);
			//virtualPallet4.PickingTasks = new List<PickingTask> { a41 };

			DbContext.PickingTasks.AddRange(a11, a12, a21, a22, a31, a41);
			DbContext.VirtualPallets.AddRange(virtualPallet1, virtualPallet2, virtualPallet3, virtualPallet4);
			await DbContext.SaveChangesAsync();
			
			// Act			
			var result = await Mediator.Send(new GetListIssueToPickingQuery(
				DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
				DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))));

			// Assert
			result.Result.Should().HaveCount(2);// Tu mi liczy klientów - dlatego 2
					
			// 1. Sprawdź, że w ogóle mamy jakiegoś klienta
			Assert.NotNull(result);
			Assert.NotEmpty(result.Result);

			// 2. Pierwszy klient istnieje
			var firstClient = result.Result.First();
			Assert.Equal(client1.Id, firstClient.ClientIdOut);

			// 3. Pierwszy klient ma dokładnie 2 zlecenia
			Assert.Equal(2, firstClient.IssuesDetailsForPicking.Count);

			// 4. Pierwsze zlecenie klienta ma 2 produkty
			var firstIssue = firstClient.IssuesDetailsForPicking.First();
			Assert.Equal(client1.Issues.First().IssueNumber, firstIssue.IssueNumber);
			Assert.Equal(2, firstIssue.Products.Count);

			// 5. Drugie zlecenie pierwszego klienta ma 1 produkt
			var secondIssue = firstClient.IssuesDetailsForPicking.Skip(1).First();
			//Assert.Equal(client1.Issues.First(x=>x.IssueNumber ==2).IssueNumber, secondIssue.IssueNumber);			
			Assert.Single(secondIssue.Products);
					
			// Klient1, Issue1, Product1 → 10 + 20 = 30
			var issue1To1Client = firstClient.IssuesDetailsForPicking.FirstOrDefault(i => i.IssueNumber == issue1.IssueNumber);
			var issue1Product1 = issue1To1Client.Products.FirstOrDefault(x => x.ProductId == product1.Id);
			var issue1Product2 = issue1To1Client.Products.FirstOrDefault(x => x.ProductId == product2.Id);
			Assert.Equal(30, issue1Product1.Quantity);
			Assert.Equal(10, issue1Product2.Quantity);

			var client1Result = result.Result.Should().ContainSingle(r => r.ClientIdOut == client1.Id).Subject;
			// Klient1, Issue2, Product2 → 15
			//var issue2Result = client1Result.IssuesDetailsForPicking.Should().ContainSingle(i => i.IssueNumber == issue2.IssueNumber).Subject;
			//issue2Result.Products.Should().ContainSingle(p => p.ProductId == product1.Id && p.Quantity == 15);

			// --- Klient 2 ---
			var client2Result = result.Result.Should().ContainSingle(r => r.ClientIdOut == client2.Id).Subject;

			// Klient2 ma 1 zlecenie
			client2Result.IssuesDetailsForPicking.Should().HaveCount(1);

			// Klient2, Issue3, Product1 → 25
			var secondClient = result.Result.Skip(1).First();	
			var issue3To2Client = secondClient.IssuesDetailsForPicking.FirstOrDefault(i=>i.IssueNumber == issue3.IssueNumber);
			var issue3Product1 = issue3To2Client.Products.FirstOrDefault(x => x.ProductId == product1.Id);
			var issue3Product2 = issue3To2Client.Products.FirstOrDefault(x => x.ProductId == product2.Id);
			Assert.Equal(25, issue3Product1.Quantity);
			Assert.Equal(15, issue3Product2.Quantity);
			//var issue3Result = client2Result.IssuesDetailsForPicking.Should().ContainSingle(i => i.IssueNumber == issue3.IssueNumber).Subject;
			//issue3Result.Products.Should().ContainSingle(p => p.ProductId == product1.Id && p.Quantity == 25);

		}
	}
}
