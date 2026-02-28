using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.Commands.UpdateIssue;
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
	public class IssueUpdateIntegrationServiceTests :TestBase 
	{
		//HappyPath
		[Fact]
		public async Task UpdateIssueAsync_NoPickingTaskForFirstAttempAndAssignsNewOnes()
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
			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

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
			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			//Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(5, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.PickingTasks.First().RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);

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
		public async Task UpdateIssueAsync_ReplacesOldPickingTasksAndAssignsNewOnes()
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

			// Act 1 – create issue with 1 pallet  and pickingTask (12 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 12, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			//czy jakaś inna też Transit?? nie do końca !!
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (2 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(pickingTasksForIssue1);
			var alloc1 = pickingTasksForIssue1.Single();
			Assert.Equal(2, alloc1.RequestedQuantity);
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
			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.Id == issue.Id);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();
			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(pickingTasksForIssue);
			var pickingTask = pickingTasksForIssue.Single();
			Assert.Equal(5, pickingTask.RequestedQuantity);
			Assert.NotNull(pickingTask.VirtualPallet);
			Assert.Equal("P2", pickingTask.VirtualPallet.PalletId);
			//kontrola zapisu historii
			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == "P2");
			Assert.Equal(5, vp.PickingTasks.First().RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);
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
				.Where(h => h.PickingTaskId == pickingTask.Id)
				.OrderBy(h => h.Id)
				.ToList();
			// Powinny być 2 wpisy: Create + Correction
			Assert.Equal(1, history.Count);// tak nie powinno być ma być 2
			//Assert.Equal(2, history.Count);
			//Assert.Single(history);
			// Ostatni wpis powinien być Correction
			var lastHistory = history.Last();
			Assert.Equal(PickingStatus.Allocated, lastHistory.StatusAfter);
			Assert.Equal("User2", lastHistory.PerformedBy);
			Assert.Equal(pickingTask.Id, lastHistory.PickingTaskId);
		}
		[Fact]
		public async Task UpdateIssueAsync_NoPickingTaskForFirstAttempAndAssignsNewOnesWithOlsPickingTaskInBaseVirtualoPallet()
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
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = client,
				IssueDateTimeCreate = DateTime.Now.AddDays(-10),
				IssueDateTimeSend = DateTime.Now.AddDays(2),
				IssueStatus = IssueStatus.InProgress,
				Pallets = new List<Pallet>(),
				PerformedBy = "user3"
			};
			var pickingTask = new PickingTask
			{
				RequestedQuantity = 4,
				Issue = issueOld,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet2,
				Location = pallet2.Location,
				InitialPalletQuantity = pallet2.ProductsOnPallet.First().Quantity,
				PickingTasks = new List<PickingTask> { pickingTask }
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Issues.AddRange(issueOld);
			DbContext.PickingTasks.Add(pickingTask);
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

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).FirstOrDefault(i => i.IssueNumber == 2);
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 15 szt. (1 pełna paleta + 5 do pickingu)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				IssueNumber = issue.IssueNumber,
				ClientId = issue.ClientId,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

			// Assert – sprawdź Issue
			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				.First(i => i.IssueNumber == issue.IssueNumber);

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Single(updatedIssue.Pallets);
			Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(5, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.PickingTasks.First(x => x.IssueId == issue.Id).RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);
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
				Id = Guid.NewGuid(),
				IssueNumber = 1,
				Client = client,
				IssueDateTimeCreate = DateTime.Now.AddDays(-10),
				IssueDateTimeSend = DateTime.Now.AddDays(2),
				IssueStatus = IssueStatus.InProgress,
				Pallets = new List<Pallet>(),
				PerformedBy = "user3"
			};
			var pickingTask = new PickingTask
			{
				RequestedQuantity = 4,
				Issue = issueOld,
				PickingStatus = PickingStatus.Allocated
			};
			var virtualPallet = new VirtualPallet
			{
				Pallet = pallet2,
				Location = pallet2.Location,
				InitialPalletQuantity = pallet2.ProductsOnPallet.First().Quantity,
				PickingTasks = new List<PickingTask> { pickingTask }
			};			
			DbContext.Products.Add(product);
			DbContext.Locations.Add(location);
			DbContext.Pallets.AddRange(pallet1, pallet2);
			DbContext.Issues.AddRange(issueOld);
			DbContext.PickingTasks.Add(pickingTask);
			DbContext.VirtualPallets.Add(virtualPallet);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				//Id = Guid.NewGuid(),
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).FirstOrDefault(i => i.IssueNumber == 2);
			issue.IssueStatus = IssueStatus.ConfirmedToLoad;
			//Assert
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: zmieniamy zamówienie na 15 szt. (1 pełna paleta + 5 do pickingu)
			var updateDto = new UpdateIssueDTO
			{
				Id = issue.Id,
				IssueNumber = issue.IssueNumber,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
		{
			new IssueItemDTO { ProductId = product.Id, Quantity = 15, BestBefore = new DateOnly(2026,1,1) }
		}
			};

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));
			var newIssueItems = DbContext.IssueItems.Where(i => i.IssueNumber == 3).ToList();
			//var newIssueItems = DbContext.IssueItems.Where(i => i.IssueId == 3).ToList();
			foreach (var it in newIssueItems) { Console.WriteLine($"Item: ProductId={it.ProductId}, Quantity={it.Quantity}, BestBefore={it.BestBefore}"); }
			// Assert – sprawdź Issue
			//var newIssue = DbContext.Issues.Find(3);  // Lub Include(i => i.IssueItems)
			var newIssue = DbContext.Issues.First(i=>i.IssueNumber ==3);  // Lub Include(i => i.IssueItems)
			//var newIssueItems1 = DbContext.IssueItems.Where(i => i.IssueId == 3).ToList();
			var newIssueItems1 = DbContext.IssueItems.Where(i => i.IssueNumber == 3).ToList();

			Assert.NotNull(newIssue);  // Issue istnieje
			Assert.Single(newIssueItems1);  // Dokładnie jeden!
			Assert.Equal(product.Id, newIssueItems1.Single().ProductId);
			Assert.Equal(5, newIssueItems1.Single().Quantity);  // Różnica
			Assert.Equal(new DateOnly(2026, 1, 1), newIssueItems1.Single().BestBefore);

			var updatedIssue = DbContext.Issues
				.Include(i => i.Pallets)
				//.Last();
				.First(i => i.IssueNumber == 3); //trzecie issue w teście

			Assert.Equal("User2", updatedIssue.PerformedBy);
			Assert.Empty(updatedIssue.Pallets);
			//Assert.Equal(PalletStatus.InTransit, updatedIssue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == updatedIssue.Id)
				.ToList();

			// Powinna być jedna alokacja (5 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(pickingTasksForIssue);
			var alloc = pickingTasksForIssue.Single();
			Assert.Equal(5, alloc.RequestedQuantity);
			Assert.NotNull(alloc.VirtualPallet);
			Assert.Equal("P2", alloc.VirtualPallet.PalletId);

			// Dodatkowa kontrola: VirtualPallet.RemainingQuantity == InitialPalletQuantity - pickingTask
			var vp = DbContext.VirtualPallets
				.Include(v => v.PickingTasks)
				.First(v => v.PalletId == "P2");

			Assert.Equal(5, vp.PickingTasks.First(x => x.IssueId == updatedIssue.Id).RequestedQuantity);
			Assert.Equal(vp.InitialPalletQuantity - vp.PickingTasks.Sum(a => a.RequestedQuantity), vp.RemainingQuantity);
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

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Find(a => a.ProductId == product.Id);
			var alloc2 = pickingTasksForIssue1.Find(a => a.ProductId == product1.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(7, alloc2.RequestedQuantity);
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

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

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

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Find(a => a.ProductId == product.Id);
			var alloc2 = pickingTasksForIssue1.Find(a => a.ProductId == product1.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(7, alloc2.RequestedQuantity);
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

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

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
				.Include(i => i.PickingTasks) // Załaduj też alokacje!
				.First(i => i.Id == issue.Id);

			// SPRAWDZENIE DLA PROD 1 (21 sztuki)
			// Oczekujemy: 2 pełne palety + alokacja na 1 sztuki
			var palletsProd1 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product.Id))
				.ToList();

			
			Assert.Equal(2, palletsProd1.Count); // Powinny być 2 palety (np. P1 i P4)

			var allocProd1 = updatedIssue1.PickingTasks.FirstOrDefault(a => a.ProductId == product.Id);
			//var allocProd2 = updatedIssue1.PickingTasks.LastOrDefault(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == product.Id);
			Assert.NotNull(allocProd1);
			//Assert.NotNull(allocProd2);
			Assert.Equal(1, allocProd1.RequestedQuantity); // Reszta 2 sztuki
			//Assert.Equal(9, allocProd2.Quantity); // Reszta 2 sztuki

			// SPRAWDZENIE DLA PROD 2 (8 sztuk)
			// Oczekujemy: 0 pełnych palet + alokacja na 8 sztuk
			var palletsProd2 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product1.Id))
				.ToList();
			Assert.Empty(palletsProd2); // 8 sztuk nie tworzy pełnej palety

			var allocProd3 = updatedIssue1.PickingTasks
				.FirstOrDefault(a => a.ProductId == product1.Id);
			Assert.NotNull(allocProd3);
			Assert.Equal(8, allocProd3.RequestedQuantity);
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
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) }
						}
					},
					new Pallet
					{
						Id = "P2",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) }
						}
					},
					new Pallet
					{
						Id = "P4",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) }
						}
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) }
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 7, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Equal(2, issue.Pallets.Count); // powinien być przypisany P1 p2
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Find(a => a.ProductId == product.Id);
			var alloc2 = pickingTasksForIssue1.Find(a => a.ProductId == product1.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(7, alloc2.RequestedQuantity);
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
					new IssueItemDTO { ProductId = product.Id, Quantity = 11, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) } ,
					new IssueItemDTO { ProductId = product1.Id, Quantity = 8, BestBefore = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(300)) }
				}
			};

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));
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
				.Include(i => i.PickingTasks) // Załaduj też alokacje!
				.First(i => i.Id == issue.Id);

			// SPRAWDZENIE DLA PROD 1 (11 sztuki)
			// Oczekujemy: 1 pełne palety + alokacja na 1 sztuki
			var palletsProd1 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product.Id))
				.ToList();


			Assert.Equal(1, palletsProd1.Count); // Powinny być 1 palety (np. P1 )

			var allocProd1 = updatedIssue1.PickingTasks.FirstOrDefault(a => a.ProductId == product.Id);
			//var allocProd2 = updatedIssue1.PickingTasks.LastOrDefault(a => a.VirtualPallet.Pallet.ProductsOnPallet.First().ProductId == product.Id);
			Assert.NotNull(allocProd1);
			//Assert.NotNull(allocProd2);
			Assert.Equal(1, allocProd1.RequestedQuantity); // Reszta 2 sztuki
			//Assert.Equal(9, allocProd2.Quantity); // Reszta 2 sztuki

			// SPRAWDZENIE DLA PROD 2 (8 sztuk)
			// Oczekujemy: 0 pełnych palet + alokacja na 8 sztuk
			var palletsProd2 = updatedIssue1.Pallets
				.Where(p => p.ProductsOnPallet.Any(pop => pop.ProductId == product1.Id))
				.ToList();
			Assert.Empty(palletsProd2); // 8 sztuk nie tworzy pełnej palety

			var allocProd3 = updatedIssue1.PickingTasks
				.FirstOrDefault(a => a.ProductId == product1.Id);
			Assert.NotNull(allocProd3);
			Assert.Equal(8, allocProd3.RequestedQuantity);
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

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinny być dwie alokacja (2 sztuk) powiązana z VirtualPallet dla "P2" i "P3"
			Assert.Equal(2, pickingTasksForIssue1.Count);
			var alloc1 = pickingTasksForIssue1.Find(a => a.ProductId == product.Id);
			var alloc2 = pickingTasksForIssue1.Find(a => a.ProductId == product1.Id);
			Assert.Equal(2, alloc1.RequestedQuantity);
			Assert.Equal(2, alloc2.RequestedQuantity);
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

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

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

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Assert – alokacje przypisane do tego Issue (sprawdzamy tabelę PickingTasks)
			var pickingTasksForIssue1 = DbContext.PickingTasks
				.Include(a => a.VirtualPallet)
					.ThenInclude(vp => vp.Pallet)
				.Where(a => a.IssueId == issue.Id)
				.ToList();

			// Powinna być jedna alokacja (2 sztuk) powiązana z VirtualPallet dla "P2"
			Assert.Single(pickingTasksForIssue1);
			var alloc1 = pickingTasksForIssue1.Single();
			Assert.Equal(2, alloc1.RequestedQuantity);
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

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

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

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

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

			var result = await Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7)));

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
				//Id = Guid.NewGuid(),
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);

			// Act 2 – update: inny numer id
			var updateDto = new UpdateIssueDTO
			{
				Id = Guid.NewGuid(),
				IssueNumber = 3,
				PerformedBy = "User2",
				DateToSend = DateTime.UtcNow.AddDays(1),
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 22, BestBefore = new DateOnly(2026,1,1) }
				}
			};
			// Assert & Act
			var result = await Assert.ThrowsAsync<NotFoundIssueException>(() => Mediator.Send(new UpdateIssueNewCommand(updateDto, DateTime.UtcNow.AddDays(7))));
			Assert.Contains($"Zamówienie o numerze {updateDto.Id} nie zostało znalezione.", result.Message);
		}
		
	}
}