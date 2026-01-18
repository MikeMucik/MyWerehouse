using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Warehouse.Models;
using Xunit.Sdk;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration
{
	public class IssueUpdateIntegrationServiceTests : IssueIntegrationCommandService
	{
		//HappyPath
		[Fact]
		public async Task UpdateIssueAsync_NoAllocationForFirstAttempAndAssignsNewOnes()
		{
			// Arrange – setup initial data
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };

			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};

			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};

			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 15 szt. (1 pełna paleta + 5 do pickingu)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = new DateOnly(2026,1,1) }
		}
			};
			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(allocationsForIssue);
			var alloc = allocationsForIssue.Single();
			Assert.Equal(5, alloc.Quantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First().Quantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);

			// ACT UpdateIssueAsync

			var p2After = DbContext.Pallets.AsNoTracking().First(p => p.Id == "P2");
			// bezpieczeństwo — potwierdzamy faktyczną zmianę statusu
			Assert.Equal(PalletStatus.ToPicking, p2After.Status);
		}
		[Fact]
		public async Task UpdateIssueAsync_ReplacesOldAllocationsAndAssignsNewOnes()
		{
			// Arrange – setup initial data
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet  and allocation (12 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue1 = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (2 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(allocationsForIssue1);
			var alloc1 = allocationsForIssue1.Single();
			Assert.Equal(2, alloc1.Quantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.Equal("P2", alloc1.VirtualPallet.PalletId);

			// Act 2 – update: zmieniamy zamówienie na 15 szt. (1 pełna paleta + 5 do pickingu)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();


			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(allocationsForIssue);
			var alloc = allocationsForIssue.Single();
			Assert.Equal(5, alloc.Quantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			//kontrola zapisu historii

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First().Quantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);



			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);

			//Assert
			var historyPallets = DbContext.PalletMovements
				//.Where(h => h.PalletId == "P2")
				.ToList();
			// Assert – historia alokacji po aktualizacji
			var history = DbContext.HistoryPickings
				.Where(h => h.AllocationId == alloc.Id)
				.OrderBy(h => h.Id)
				.ToList();

			// Powinny być 2 wpisy: Create + Correction
			Assert.Single(history);

			// Ostatni wpis powinien być Correction
			var lastHistory = history.Last();

			Assert.Equal(PickingStatus.Allocated, lastHistory.StatusAfter);
			Assert.Equal("User2", lastHistory.PerformedBy);
			Assert.Equal(alloc.Id, lastHistory.AllocationId);

		}
		[Fact]
		public async Task UpdateIssueAsync_NoAllocationForFirstAttempAndAssignsNewOnesWithOlsAllocationInBaseVirtualoPallet()
		{
			// Arrange – setup initial data
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallet1 = new Pallet
			{
				Id = "P1",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
			};
			var pallet2 = new Pallet
			{
				Id = "P2",
				Location = location,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
			};
			var issueOld = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.Now.AddDays(-10),
				IssueDateTimeSend = DateTime.Now.AddDays(2),
				IssueStatus = IssueStatus.InProgress,
				Pallets = new List<Pallet>(),
				PerformedBy = "user3"
			};
			var allocation = new Allocation
			{
				Quantity = 4,
				Issue = issueOld,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet2,
				Location = pallet2.Location,
				InitialPalletQuantity = pallet2.ProductsOnPallet.First().Quantity,
				Allocations = new List<Allocation> { allocation }
			};
			//allocation.VirtualPallet = virtualPallet;
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Issues.AddRange(issueOld);
			DbContext.Allocations.Add(allocation);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).FirstOrDefault(i => i.Id == 2);
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 15 szt. (1 pełna paleta + 5 do pickingu)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(allocationsForIssue);
			var alloc = allocationsForIssue.Single();
			Assert.Equal(5, alloc.Quantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First(x => x.IssueId == issue.Id).Quantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);
			Assert.Equal(1, vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);
		}
		[Fact]
		public async Task UpdateIssueAsync_IssueConfirmedAndAllocatedProductOnPallet_MakeNewIssue()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallet1 = new Pallet
			{
				Id = "P1",
				Location = location,
				Status = PalletStatus.Available,
				ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
			};
			var pallet2 = new Pallet
			{
				Id = "P2",
				Location = location,
				Status = PalletStatus.ToPicking,
				ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
			};
			var issueOld = new Issue
			{
				Client = client,
				IssueDateTimeCreate = DateTime.Now.AddDays(-10),
				IssueDateTimeSend = DateTime.Now.AddDays(2),
				IssueStatus = IssueStatus.InProgress,
				Pallets = new List<Pallet>(),
				PerformedBy = "user3"
			};
			var allocation = new Allocation
			{
				Quantity = 4,
				Issue = issueOld,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet2,
				Location = pallet2.Location,
				InitialPalletQuantity = pallet2.ProductsOnPallet.First().Quantity,
				Allocations = new List<Allocation> { allocation }
			};
			//allocation.VirtualPallet = virtualPallet;
			//DbContext.Clients.Add(client);
			//DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Issues.AddRange(issueOld);
			DbContext.Allocations.Add(allocation);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).FirstOrDefault(i => i.Id == 2);
			issue.IssueStatus = IssueStatus.ConfirmedToLoad;
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 15 szt. (1 pełna paleta + 5 do pickingu)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));
			var newIssueItems = DbContext.IssueItems.Where(i => i.IssueId == 3).ToList();
			foreach (var it in newIssueItems) { Console.WriteLine($"Item: ProductId={it.ProductId}, Quantity={it.Quantity}, BestBefore={it.BestBefore}"); }
			// Assert – sprawdź Issue

			var newIssue = DbContext.Issues.Find(3);  // Lub Include(i => i.IssueItems)
			var newIssueItems1 = DbContext.IssueItems.Where(i => i.IssueId == 3).ToList();

			Assert.NotNull(newIssue);  // Issue istnieje
			Assert.Single(newIssueItems1);  // Dokładnie jeden!
			Assert.Equal(product.Id, newIssueItems1.Single().ProductId);
			Assert.Equal(5, newIssueItems1.Single().Quantity);  // Różnica
			Assert.Equal(new DateOnly(2026, 1, 1), newIssueItems1.Single().BestBefore);

			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == 3); //trzecie issue w teście

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Empty(updatedIssue.Pallets);
			//Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == updatedIssue.Id)
				.ToList();

			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(allocationsForIssue);
			var alloc = allocationsForIssue.Single();
			Assert.Equal(5, alloc.Quantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First(x => x.IssueId == updatedIssue.Id).Quantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);
			Assert.Equal(1, vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);
		}

		[Fact]
		public async Task UpdateIssueAsync_InsufficientStaffForOneCheckSecond_Success()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var product1 = new Product
			{
				Name = "Prod2",
				SKU = "SKU2",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = new DateOnly(2026,1,1) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 7, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue1 = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, allocationsForIssue1.Count);
			var alloc1 = allocationsForIssue1.First();
			var alloc2 = allocationsForIssue1.Last();
			Assert.Equal(2, alloc1.Quantity);
			Assert.Equal(7, alloc2.Quantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal("P2", alloc1.VirtualPallet.PalletId);
			Assert.Equal("P3", alloc2.VirtualPallet.PalletId);

			// Act 2 – update: zmieniamy zamówienie na 22 szt. (brak towaru)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),

				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) }	,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 8, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Equal(2, result.Count);
			Assert.False(result.First().Success);
			Assert.True(result.Last().Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}", result.First().Message);
			Assert.Equal(product.Id, result.First().ProductId);
			Assert.Equal(product1.Id, result.Last().ProductId);
		}

		[Fact]
		public async Task UpdateIssueAsync_InsufficientStaffForOneAddMoreOne_Success()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var product1 = new Product
			{
				Name = "Prod2",
				SKU = "SKU2",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P4",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = new DateOnly(2026,1,1) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 7, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue1 = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, allocationsForIssue1.Count);
			var alloc1 = allocationsForIssue1.First();
			var alloc2 = allocationsForIssue1.Last();
			Assert.Equal(2, alloc1.Quantity);
			Assert.Equal(7, alloc2.Quantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal("P2", alloc1.VirtualPallet.PalletId);
			Assert.Equal("P3", alloc2.VirtualPallet.PalletId);

			// Act 2 – update: zmieniamy zamówienie na 22 szt. (brak towaru)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),

				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 21, BestBefore = new DateOnly(2026,1,1) } ,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 8, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Equal(2, result.Count);
			Assert.True(result.First().Success);
			Assert.True(result.Last().Success);
			Assert.Contains("Towar dołączono do wydania", result.First().Message);
			Assert.Contains("Towar dołączono do wydania", result.Last().Message);
			Assert.Equal(product.Id, result.First().ProductId);
			Assert.Equal(product1.Id, result.Last().ProductId);

			var updatedIssue1 = DbContext.Issues
				.Include(i => i.Pallets)
				.Include(i => i.Allocations) // Załaduj też alokacje!
				.First(i => i.Id == issue.Id);

			// SPRAWDZENIE DLA PROD 1 (21 sztuki)
			// Oczekujemy: 2 pełne palety + alokacja na 1 sztuki
			var palletsProd1 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product.Id))
				.ToList();

			
			Assert.Equal(2, palletsProd1.Count); // Powinny być 2 palety (np. P1 i P4)

			var allocProd1 = updatedIssue1.Allocations.FirstOrDefault(a => a.ProductId == product.Id);
			//var allocProd2 = updatedIssue1.Allocations.LastOrDefault(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == product.Id);
			Assert.NotNull(allocProd1);
			//Assert.NotNull(allocProd2);
			Assert.Equal(1, allocProd1.Quantity); // Reszta 2 sztuki
			//Assert.Equal(9, allocProd2.Quantity); // Reszta 2 sztuki

			// SPRAWDZENIE DLA PROD 2 (8 sztuk)
			// Oczekujemy: 0 pełnych palet + alokacja na 8 sztuk
			var palletsProd2 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product1.Id))
				.ToList();
			Assert.Empty(palletsProd2); // 8 sztuk nie tworzy pełnej palety

			var allocProd3 = updatedIssue1.Allocations
				.FirstOrDefault(a => a.ProductId == product1.Id);
			Assert.NotNull(allocProd3);
			Assert.Equal(8, allocProd3.Quantity);
		}

		[Fact]
		public async Task UpdateIssueAsync_ProperStaffReduce_Success()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var product1 = new Product
			{
				Name = "Prod2",
				SKU = "SKU2",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P4",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 7, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Equal(2, issue.Pallets.Count); // powinien być przypisany P1 p2
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue1 = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, allocationsForIssue1.Count);
			var alloc1 = allocationsForIssue1.First();
			var alloc2 = allocationsForIssue1.Last();
			Assert.Equal(2, alloc1.Quantity);
			Assert.Equal(7, alloc2.Quantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal("P4", alloc1.VirtualPallet.PalletId);
			Assert.Equal("P3", alloc2.VirtualPallet.PalletId);

			// Act 2 – update: zmieniamy zamówienie na 11 szt. 
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),

				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 11, BestBefore = new DateOnly(2026,1,1) } ,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 8, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));
			//var result1 = await _m
			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Equal(2, result.Count);
			Assert.True(result.First().Success);
			Assert.True(result.Last().Success);
			Assert.Contains("Towar dołączono do wydania", result.First().Message);
			Assert.Contains("Towar dołączono do wydania", result.Last().Message);
			Assert.Equal(product.Id, result.First().ProductId);
			Assert.Equal(product1.Id, result.Last().ProductId);

			var updatedIssue1 = DbContext.Issues
				.Include(i => i.Pallets)
				.Include(i => i.Allocations) // Załaduj też alokacje!
				.First(i => i.Id == issue.Id);

			// SPRAWDZENIE DLA PROD 1 (11 sztuki)
			// Oczekujemy: 1 pełne palety + alokacja na 1 sztuki
			var palletsProd1 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product.Id))
				.ToList();


			Assert.Equal(1, palletsProd1.Count); // Powinny być 1 palety (np. P1 )

			var allocProd1 = updatedIssue1.Allocations.FirstOrDefault(a => a.ProductId == product.Id);
			//var allocProd2 = updatedIssue1.Allocations.LastOrDefault(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == product.Id);
			Assert.NotNull(allocProd1);
			//Assert.NotNull(allocProd2);
			Assert.Equal(1, allocProd1.Quantity); // Reszta 2 sztuki
			//Assert.Equal(9, allocProd2.Quantity); // Reszta 2 sztuki

			// SPRAWDZENIE DLA PROD 2 (8 sztuk)
			// Oczekujemy: 0 pełnych palet + alokacja na 8 sztuk
			var palletsProd2 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product1.Id))
				.ToList();
			Assert.Empty(palletsProd2); // 8 sztuk nie tworzy pełnej palety

			var allocProd3 = updatedIssue1.Allocations
				.FirstOrDefault(a => a.ProductId == product1.Id);
			Assert.NotNull(allocProd3);
			Assert.Equal(8, allocProd3.Quantity);
		}

		[Fact]
		public async Task UpdateIssueAsync_InsufficientStaffForOne_Success()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var product1 = new Product
			{
				Name = "Prod2",
				SKU = "SKU2",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = new DateOnly(2026,1,1) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 2, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue1 = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, allocationsForIssue1.Count);
			var alloc1 = allocationsForIssue1.First();
			var alloc2 = allocationsForIssue1.Last();
			Assert.Equal(2, alloc1.Quantity);
			Assert.Equal(2, alloc2.Quantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.NotNull(alloc2.VirtualPallet);
			Assert.Equal("P2", alloc1.VirtualPallet.PalletId);
			Assert.Equal("P3", alloc2.VirtualPallet.PalletId);

			// Act 2 – update: zmieniamy zamówienie na 22 szt. (brak towaru)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),

				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) } ,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 3, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Equal(2, result.Count);
			Assert.False(result.First().Success);
			Assert.True(result.Last().Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}", result.First().Message);
			Assert.Equal(product.Id, result.First().ProductId);
			Assert.Equal(product1.Id, result.Last().ProductId);
		}

		//SadPath
		[Fact]
		public async Task UpdateIssueAsync_InsufficientStaff()
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
			var client = new Client
			{
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę Allocations)
			var allocationsForIssue1 = DbContext.Allocations
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (2 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(allocationsForIssue1);
			var alloc1 = allocationsForIssue1.Single();
			Assert.Equal(2, alloc1.Quantity);
			Assert.NotNull(alloc1.VirtualPallet);
			Assert.Equal("P2", alloc1.VirtualPallet.PalletId);

			// Act 2 – update: zmieniamy zamówienie na 22 szt. (brak towaru)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),

				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			//Assert.Equal("User1", updatedIssue.PerformedBy); //akcja nieudana więc użytkownik z poprzedniej zmiany

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.False(result.First().Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}", result.First().Message);
			Assert.Equal(product.Id, result.First().ProductId);
		}
		[Fact]
		public async Task UpdateIssueAsync_WrongStatusPallet()
		{
			// Arrange – setup initial data
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.OnHold,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 22 szt. (brak towaru)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User1", updatedIssue.PerformedBy); //akcja nieudana więc użytkownik z poprzedniej zmiany

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.False(result.First().Success);
			Assert.Contains($"Nie wystarczająca ilości produktu o numerze {product.Id}. Asortyment nie został dodany do zlecenia.", result.First().Message);
			Assert.Equal(product.Id, result.First().ProductId);
		}
		[Fact]
		public async Task UpdateIssueAsync_NotIssue_ThrowsException()
		{
			// Arrange – setup initial data
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
				Name = "TestCompany",
				Email = "123@op.pl",
				Description = "Description",
				FullName = "FullNameCompany",
				Addresses = new List<Address> { address }
			};
			var category = new Category { Name = "Cat" };
			var location = new Location { Aisle = 1, Bay = 1, Height = 1, Position = 1 };
			var product = new Product
			{
				Name = "Prod1",
				SKU = "SKU1",
				Category = category,
				CartonsPerPallet = 10
			};
			var pallets = new List<Pallet>
				{
					new Pallet
					{
						Id = "P1",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.OnHold,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallets);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await _issueService.CreateNewIssueAsync(createIssueDto, DateTime.UtcNow.AddDays(7));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 22 szt. (brak towaru)
			var updateDto = new UpdateIssueDTO
			{
				Id = 2,
				//issue.Id,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) }
				}
			};
			// Assert & Act
			var result = await Assert.ThrowsAsync<NotFoundIssueException>(() => _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7)));
			Assert.Contains($"Zamówienie o numerze {updateDto.Id} nie zostało znalezione.", result.Message);
		}
		//[Fact]
		//public async Task UpdateIssueAsync_WhenFirstProductFailsAfterSync_ShouldNotPersistDirtyData_AndSaveSecondProduct()
		//{
		//	// --- ARRANGE ---

		//	// 1. Setup Danych (2 produkty, 2 palety)
		//	var productFail = new Product { Id = 1, Name = "FailProd", SKU = "F1", CartonsPerPallet = 10 };
		//	var productSuccess = new Product { Id = 2, Name = "OkProd", SKU = "S1", CartonsPerPallet = 10 };

		//	var palletForFail = new Pallet { Id = "P_Fail", Status = PalletStatus.Available, ProductsOnPallet = new() { new() { Product = productFail, Quantity = 10 } } };
		//	var palletForSuccess = new Pallet { Id = "P_Ok", Status = PalletStatus.Available, ProductsOnPallet = new() { new() { Product = productSuccess, Quantity = 10 } } };

		//	var issue = new Issue
		//	{
		//		Id = 1,
		//		IssueStatus = IssueStatus.New,
		//		ClientId = 1,
		//		Pallets = new List<Pallet>() // Pusta lista na start
		//	};

		//	// Dodajemy do bazy (zakładam, że DbContext to prawdziwa instancja z SQLite)
		//	DbContext.Products.AddRange(productFail, productSuccess);
		//	DbContext.Pallets.AddRange(palletForFail, palletForSuccess);
		//	DbContext.Issues.Add(issue);
		//	await DbContext.SaveChangesAsync();

		//	// 2. Setup Mocków Mediatora (Symulujemy, że logika biznesowa zwraca palety)
		//	// Mockujemy zapytania o dostępność, żeby zawsze zwracały OK
		//	_mediatorMock.Setup(m => m.Send(It.IsAny<GetProductCountQuery>(), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(100);
		//	_mediatorMock.Setup(m => m.Send(It.IsAny<GetNumberPalletsAndRestQuery>(), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new AssignPallestResult(1, 0)); // 1 pełna paleta

		//	// Ważne: Zwracamy odpowiednie palety dla odpowiednich produktów
		//	_mediatorMock.Setup(m => m.Send(It.Is<GetAvailablePalletsByProductQuery>(q => q.ProductId == productFail.Id), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new List<Pallet> { palletForFail });

		//	_mediatorMock.Setup(m => m.Send(It.Is<GetAvailablePalletsByProductQuery>(q => q.ProductId == productSuccess.Id), It.IsAny<CancellationToken>()))
		//		.ReturnsAsync(new List<Pallet> { palletForSuccess });

		//	// Mockujemy przypisanie palet (to, co zwraca assigned pallets)
		//	_mediatorMock.Setup(m => m.Send(It.Is<AssignFullPalletToIssueCommand>(c => c.Issue.Id == issue.Id), It.IsAny<CancellationToken>()))
		//		.Returns<AssignFullPalletToIssueCommand, CancellationToken>((cmd, ct) => Task.FromResult(cmd.FreePallets)); // Zwracamy te same palety jako przypisane

		//	// --- PUŁAPKA (THE TRAP) ---
		//	// Symulujemy, że EventCollector ma jakieś zdarzenia, żeby wejść w pętlę foreach(evn in ...)
		//	_eventCollector.Events.Add(new SomeDomainEvent());

		//	// Konfigurujemy Mediatora tak, aby RZUCAŁ WYJĄTEK tylko przy publikacji eventu, 
		//	// ale tylko w pierwszej iteracji (dla productFail).
		//	// Ponieważ kod przetwarza produkty w pętli, musimy zidentyfikować moment.
		//	// Najprościej: rzuć wyjątek, jeśli issue.Pallets zawiera "P_Fail".

		//	_mediatorMock.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
		//		.Callback(() =>
		//		{
		//			// Sprawdzamy stan bazy/obiektu w momencie publikacji eventu
		//			// Jeśli aktualnie przetwarzamy FailProd, to synchronizer już dodał paletę P_Fail do issue.
		//			var currentIssue = DbContext.Issues.Local.FirstOrDefault(i => i.Id == 1);
		//			if (currentIssue != null && currentIssue.Pallets.Any(p => p.Id == "P_Fail"))
		//			{
		//				throw new Exception("BUM! Błąd po synchronizacji, ale przed Commitem!");
		//			}
		//		});

		//	// 3. Act - Wywołanie metody
		//	var updateDto = new UpdateIssueDTO
		//	{
		//		Id = issue.Id,
		//		PerformedBy = "Tester",
		//		Items = new List<IssueItemDTO>
		//{
		//	new IssueItemDTO { ProductId = productFail.Id, Quantity = 10 },    // To ma się wywalić
		//          new IssueItemDTO { ProductId = productSuccess.Id, Quantity = 10 }  // To ma przejść
		//      }
		//	};

		//	// Wywołujemy serwis
		//	// Ignorujemy fakt, że metoda może zwrócić listę z błędami - interesuje nas stan bazy po operacji
		//	var results = await _issueService.UpdateIssueAsync(updateDto, DateTime.UtcNow.AddDays(7));

		//	// --- ASSERT ---

		//	// Pobieramy zlecenie "na świeżo" z nowym kontekstem lub reloadem, żeby zobaczyć co naprawdę siedzi w bazie
		//	DbContext.ChangeTracker.Clear();
		//	var issueFromDb = await DbContext.Issues.Include(i => i.Pallets).FirstAsync(i => i.Id == 1);

		//	// OCZEKIWANIA:
		//	// 1. Paleta P_Fail NIE powinna być przypisana (bo był Rollback).
		//	// 2. Paleta P_Ok POWINNA być przypisana (bo druga iteracja się udała).

		//	// DLACZEGO TWÓJ STARY KOD TU PADNIE?
		//	// Bez ReloadAsync w catchu, paleta P_Fail zostanie w pamięci RAM obiektu Issue.
		//	// Przy drugiej iteracji (dla P_Ok), EF Core zobaczy w kolekcji Pallets dwie palety: P_Fail i P_Ok.
		//	// Mimo że P_Fail była wycofana transakcją, EF spróbuje ją zapisać ZNOWU przy okazji zapisywania P_Ok.
		//	// Efekt: W bazie będą DWIE palety, mimo że dla pierwszej poleciał błąd.

		//	Assert.DoesNotContain(issueFromDb.Pallets, p => p.Id == "P_Fail"); // To obleje stary kod
		//	Assert.Contains(issueFromDb.Pallets, p => p.Id == "P_Ok");
		//	Assert.Equal(1, issueFromDb.Pallets.Count); // Powinna być tylko 1
		//}
	}
}