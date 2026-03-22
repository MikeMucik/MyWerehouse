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
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),				
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 50
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "667",
				AddedItemAd = new DateTime(2025, 2, 2),
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
			var pallet1 = new Pallet
			{
				PalletNumber = "Q10",
				DateReceived = new DateTime(2025, 8, 8),				
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product = product1,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var pallet2 = new Pallet
			{
				PalletNumber = "Q11",
				DateReceived = new DateTime(2025, 9, 9),				
				Location = location2,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{	
						Product = product1,
						Quantity = 50,
						DateAdded = new DateTime(2025, 9, 9)
					}
				}
			};
			var pallet3 = new Pallet
			{
				PalletNumber = "Q12",
				DateReceived = new DateTime(2025, 10, 10),
				Location = location3,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product = product2,
						Quantity = 70,
						DateAdded = new DateTime(2025, 10, 10)
					}
				}
			};
			var pallet4 = new Pallet
			{
				PalletNumber = "Q13",
				DateReceived = new DateTime(2025, 11, 11),
				Location = location4,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{
						Product= product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 11, 11) }
				}
			};
			var issue1 = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = client1,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue2 = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 2,
				Client = client1,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue3 = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 3,
				Client = client2,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			

			DbContext.Addresses.AddRange(address1, address2);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, location3, location4);
			DbContext.Clients.AddRange(client1, client2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Issues.AddRange(issue1, issue2, issue3);			
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = pallet1,
				InitialPalletQuantity = 40,
				Location = pallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a11 = new PickingTask { Issue = issue1, RequestedQuantity = 10, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet1, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			var a12 = new PickingTask { Issue = issue2, RequestedQuantity = 15, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet1, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			virtualPallet1.PickingTasks = new List<PickingTask> { a11, a12 };

			var virtualPallet2 = new VirtualPallet
			{
				Pallet = pallet2,
				InitialPalletQuantity = 50,
				Location = pallet2.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a21 = new PickingTask { Issue = issue1, RequestedQuantity = 20, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet2, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			var a22 = new PickingTask { Issue = issue3, RequestedQuantity = 25, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet2, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			virtualPallet2.PickingTasks = new List<PickingTask> { a21, a22 };

			var virtualPallet3 = new VirtualPallet
			{
				Pallet = pallet3,
				InitialPalletQuantity = 50,
				Location = pallet3.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a31 = new PickingTask { Issue = issue2, RequestedQuantity = 15, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet3, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			virtualPallet3.PickingTasks = new List<PickingTask> { a31 };

			var virtualPallet4 = new VirtualPallet
			{
				Pallet = pallet4,
				InitialPalletQuantity = 40,
				Location = pallet4.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a41 = new PickingTask { Issue = issue1, RequestedQuantity = 10, PickingStatus = PickingStatus.Allocated, VirtualPallet = virtualPallet4, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			virtualPallet4.PickingTasks = new List<PickingTask> { a41 };

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
				ProductId = product1.Id,
				Quantity = 30
			});

			// Client1, Issue2, Product2 → 15
			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client1.Id,
				IssueId = issue2.Id,
				ProductId = product2.Id,
				Quantity = 15
			});

			// Client2, Issue3, Product1 → 25
			result.Result.Result.Should().ContainEquivalentOf(new ProductToIssueDTO
			{
				ClientIdOut = client2.Id,
				IssueId = issue3.Id,
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
			var product1 = new Product
			{
				Name = "Prod A",
				SKU = "666",
				AddedItemAd = new DateTime(2025, 1, 1),				
				Category = category,
				IsDeleted = false,
				CartonsPerPallet = 50
			};
			var product2 = new Product
			{
				Name = "Prod B",
				SKU = "667",
				AddedItemAd = new DateTime(2025, 2, 2),				
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

			var pallet1 = new Pallet
			{
				PalletNumber = "Q10",
				DateReceived = new DateTime(2025, 8, 8),				
				Location = location1,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product = product1,
						Quantity = 40,
						DateAdded = new DateTime(2025, 8, 8) }
				}
			};
			var pallet2 = new Pallet
			{
				PalletNumber = "Q11",
				DateReceived = new DateTime(2025, 9, 9),				
				Location = location2,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{							
						Product = product1,
						Quantity = 50,
						DateAdded = new DateTime(2025, 9, 9)
					}
				}
			};
			var pallet3 = new Pallet
			{
				PalletNumber = "Q12",
				DateReceived = new DateTime(2025, 10, 10),
				Location = location3,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product = product2,
						Quantity = 70,
						DateAdded = new DateTime(2025, 10, 10)
					}
				}
			};
			var pallet4 = new Pallet
			{
				PalletNumber = "Q13",
				DateReceived = new DateTime(2025, 11, 11),
				Location = location4,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
				{
					new ProductOnPallet
					{						
						Product= product2,
						Quantity = 100,
						DateAdded = new DateTime(2025, 11, 11) }
				}
			};
			var issue1 = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber = 101,
				Client = client1,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue2 = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber =102,
				Client = client1,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};
			var issue3 = new Issue
			{
				Id = Guid.NewGuid(),
				IssueNumber =103,
				Client = client2,
				IssueDateTimeCreate = DateTime.UtcNow,
				IssueDateTimeSend = DateTime.UtcNow.AddDays(7),
				IssueStatus = IssueStatus.New,
				PerformedBy = "TestUser",
			};

			DbContext.Addresses.AddRange(address1, address2);
			DbContext.Categories.Add(category);
			DbContext.Locations.AddRange(location1, location2, location3, location4);
			DbContext.Clients.AddRange(client1, client2);
			DbContext.Products.AddRange(product1, product2);
			DbContext.Pallets.AddRange(pallet1, pallet2, pallet3, pallet4);
			DbContext.Issues.AddRange(issue1, issue2, issue3);
			await DbContext.SaveChangesAsync();
			var virtualPallet1 = new VirtualPallet
			{
				Pallet = pallet1,
				InitialPalletQuantity = 40,
				Location = pallet1.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a11 = new PickingTask { Issue = issue1, IssueNumber=101, RequestedQuantity = 10, PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet1, ProductId = product1.Id, PickingDay=DateOnly.FromDateTime( DateTime.UtcNow.AddDays(5)) };
			var a12 = new PickingTask { Issue = issue2, IssueNumber= 102, RequestedQuantity = 15, PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet1, ProductId = product1.Id, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
			};
			virtualPallet1.PickingTasks = new List<PickingTask> { a11, a12 };

			var virtualPallet2 = new VirtualPallet
			{
				Pallet = pallet2,
				InitialPalletQuantity = 50,
				Location = pallet2.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a21 = new PickingTask { Issue = issue1, IssueNumber=101, RequestedQuantity = 20, PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet2, ProductId = product1.Id, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
			};
			var a22 = new PickingTask { Issue = issue3, IssueNumber= 103, RequestedQuantity = 25, PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet2, ProductId = product1.Id, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
			};
			virtualPallet2.PickingTasks = new List<PickingTask> { a21, a22 };

			var virtualPallet3 = new VirtualPallet
			{
				Pallet = pallet3,
				InitialPalletQuantity = 50,
				Location = pallet3.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a31 = new PickingTask { Issue = issue3,IssueNumber =103, RequestedQuantity = 15, PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet3, ProductId = product2.Id, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5))
			};
			virtualPallet3.PickingTasks = new List<PickingTask> { a31 };

			var virtualPallet4 = new VirtualPallet
			{
				Pallet = pallet4,
				InitialPalletQuantity = 40,
				Location = pallet4.Location,
				DateMoved = new DateTime(2025, 8, 12),
			};

			var a41 = new PickingTask { Issue = issue1, IssueNumber =101, RequestedQuantity = 10, PickingStatus = PickingStatus.Allocated,
				VirtualPallet = virtualPallet4, ProductId = product2.Id, PickingDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)) };
			virtualPallet4.PickingTasks = new List<PickingTask> { a41 };

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
