using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Models;
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

			var result = await _issueService.UpdateIssueAsync(updateDto);

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

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == IssueInitialQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First().Quantity);
			Assert.Equal(vp.IssueInitialQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);
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

			var result = await _issueService.UpdateIssueAsync(updateDto);

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

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == IssueInitialQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First().Quantity);
			Assert.Equal(vp.IssueInitialQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);
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
				IssueInitialQuantity = pallet2.ProductsOnPallet.First().Quantity,
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

			var issue = DbContext.Issues.Include(i => i.Pallets).FirstOrDefault(i=>i.Id ==2);
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

			var result = await _issueService.UpdateIssueAsync(updateDto);

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

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == IssueInitialQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First(x=>x.IssueId == issue.Id).Quantity);
			Assert.Equal(vp.IssueInitialQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);
			Assert.Equal(1, vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);
		}
		[Fact]
		public async Task UpdateIssueAsync_IssueConfirmedAndAllocatedProductOnPallet_MakeNewIssue()
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
				IssueInitialQuantity = pallet2.ProductsOnPallet.First().Quantity,
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
			issue.IssueStatus= IssueStatus.ConfirmedToLoad;
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

			var result = await _issueService.UpdateIssueAsync(updateDto);
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

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == IssueInitialQuantity - allocation
			var vp = DbContext.VirtualPallets
				.Include(v => v.Allocations)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.Allocations.First(x => x.IssueId == updatedIssue.Id).Quantity);
			Assert.Equal(vp.IssueInitialQuantity - vp.Allocations.Sum(a => a.Quantity), vp.RemainingQuantity);
			Assert.Equal(1, vp.RemainingQuantity);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.True(result.First().Success);
			Assert.Equal(product.Id, result.First().ProductId);
		}


		//SadPath
		[Fact]
		public async Task UpdateIssueAsync_InsufficientStaff()
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

			var result = await _issueService.UpdateIssueAsync(updateDto);

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.False(result.First().Success);
			Assert.Contains($"Produkt o numerze {product.Id} nie został dodany do zlecenia edytuj zlecenie!", result.First().Message);
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

			var result = await _issueService.UpdateIssueAsync(updateDto);

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);

			// Wynik metody UpdateIssueAsync powinien zawierać rezultat dla produktu
			Assert.Single(result);
			Assert.False(result.First().Success);
			Assert.Contains($"Produkt o numerze {product.Id} nie został dodany do zlecenia edytuj zlecenie!", result.First().Message);
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
			var result = await Assert.ThrowsAsync<IssueNotFoundException>(() =>  _issueService.UpdateIssueAsync(updateDto));
			Assert.Contains($"Zamówienie o numerze {updateDto.Id} nie zostało znalezione.", result.Message);			
		}
	}
}
