using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.IssueServiceTests.Integration;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.ReversePickingServiceTests.Integration
{
	public class ExecutiveReversePickingTests : TestBase
	{		
		[Fact]
		public async Task ReversePickingExecute_BackToSourcePallet_ShouldRestorePalletsAvailabilityAndDoneReversePicking()
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
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
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
			var recipt = new Receipt
			{
				ReceiptNumber = 1,
				ReceiptDateTime = DateTime.UtcNow.AddDays(-1),
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "UserMakae",
				Client = client,
				Pallets = pallets,
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallets);
			DbContext.Receipts.Add(recipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = "P2",
				ProductId = product.Id,

			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);

			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);

			// Assert – Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.Id == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(1, palletP2.LocationId);
			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);

			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.ReturnToSource,
				"Q0001", "UserReverse", null));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar do palety źródłowej", resultReversePicking.Result.Message);
			Assert.Equal("P2", resultReversePicking.Result.PalletId);
			var palletAfterreversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.Id == "P2");
			Assert.NotNull(palletAfterreversePicking);
			Assert.Equal(10, palletAfterreversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterreversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);
		}
		[Fact]
		public async Task ReversePickingExecute_ProductToNewPallet_ShouldRestorePalletsAvailabilityAndDoneReversePicking()
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
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
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
			var recipt = new Receipt
			{
				Id = Guid.NewGuid(),
				ReceiptNumber = 1,
				ReceiptDateTime = DateTime.UtcNow.AddDays(-1),
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "UserMakae",
				Client = client,
				Pallets = pallets,
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallets);
			DbContext.Receipts.Add(recipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				//Id = pickingFromBase.PickingTaskNumber,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = "P2",
				ProductId = product.Id,

			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);

			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);

			// Assert – Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.Id == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(1, palletP2.LocationId);
			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 wykonanie dekompletacji
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToNewPallet,
				"Q0001", "UserReverse", null));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar do nowej palety.", resultReversePicking.Result.Message);
			Assert.Equal("Q0002", resultReversePicking.Result.PalletId);
			var palletAfterreversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.Id == "Q0002");
			Assert.NotNull(palletAfterreversePicking);
			Assert.Equal(8, palletAfterreversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.InStock, palletAfterreversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);

			var history = DbContext.HistoryPickings;
			Assert.NotNull(history);
			var history1 = DbContext.HistoryReversePickings;
			Assert.NotNull(history1);
			Assert.Equal(2, history1.Count());
		}
		[Fact]
		public async Task ReversePickingExecute_ProductToExistPallet_ShouldRestorePalletsAvailabilityAndDoneReversePicking()
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
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
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
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 1, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			var recipt = new Receipt
			{
				Id = Guid.NewGuid(),
				ReceiptNumber =1,
				ReceiptDateTime = DateTime.UtcNow.AddDays(-1),
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "UserMakae",
				Client = client,
				Pallets = pallets,
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.Add(product);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallets);
			DbContext.Receipts.Add(recipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = "P2",
				ProductId = product.Id,

			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			//Assert
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.Id == pickingFromBase.Id);
			Assert.NotNull(pickingTaskDone);


			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);

			// Assert – Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.Id == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(1, palletP2.LocationId);
			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 wykonanie dekompletacji
			var list = new List<Pallet>();
			var existingPallet = DbContext.Pallets.Find("P3");
			list.Add(existingPallet);
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToExistingPallet,
				"Q0001", "UserReverse", list));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar.", resultReversePicking.Result.Message);			
			var palletAfterreversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.Id == "P3");
			Assert.Contains(palletAfterreversePicking, resultReversePicking.Result.PalletWithAddedProduct);
			Assert.NotNull(palletAfterreversePicking);
			Assert.Equal(9, palletAfterreversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterreversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);
		}
		[Fact]
		public async Task ReversePickingExecute_ProductToExistPallet_ShouldRestorePalletsAvailabilityAndDoneReversePickingNotArchaivePickingPallet()
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
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
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
							new ProductOnPallet { Product = product, Quantity = 1, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P4",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			var recipt = new Receipt
			{
				Id = Guid.NewGuid(),
				ReceiptNumber =1,
				ReceiptDateTime = DateTime.UtcNow.AddDays(-1),
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "UserMakae",
				Client = client,
				Pallets = pallets,
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallets);
			DbContext.Receipts.Add(recipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 4, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);
			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			var palletP4 = await DbContext.Pallets.FirstAsync(p => p.Id == "P4");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = "P2",
				ProductId = product.Id,

			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			//Assert 2
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product.Id);
			Assert.NotNull(pickingTaskDone);

			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);

			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.Id == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);
			//Status.ToPicking bo wykonany
			var palletP2_3 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2_3.IssueId);
			Assert.Equal(1, palletP2_3.LocationId);
			var palletP4_3 = await DbContext.Pallets.FirstAsync(p => p.Id == "P4");
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP4_3.IssueId);
			Assert.Equal(1, palletP4_3.LocationId);
			// Assert 3– pickingTasks left 
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert 3– Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 wykonanie dekompletacji
			var list = new List<Pallet>();
			var existingPallet = DbContext.Pallets.Find("P3");
			list.Add(existingPallet);
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToExistingPallet,
				"Q0001", "UserReverse", list));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar.", resultReversePicking.Result.Message);
			var palletAfterreversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.Id == "P3");
			Assert.Contains(palletAfterreversePicking, resultReversePicking.Result.PalletWithAddedProduct);
			Assert.NotNull(palletAfterreversePicking);
			Assert.Equal(9, palletAfterreversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterreversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Archived, pickingPalletAfterReverse.Status);
		}

		[Fact]
		public async Task ReversePickingTwoExecute_ProductToExistPallet_ShouldRestorePalletsAvailabilityAndDoneOneReversePickingNotArchaivePickingPallet()
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
			var location1 = new Location { Id = 100100, Aisle = 10, Bay = 1, Height = 1, Position = 1 };
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
							new ProductOnPallet { Product = product, Quantity = 1, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P4",
						Location = location,
						Status = PalletStatus.Available,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product1, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			var recipt = new Receipt
			{
				Id = Guid.NewGuid(),
				ReceiptNumber = 1,
				ReceiptDateTime = DateTime.UtcNow.AddDays(-1),
				ReceiptStatus = ReceiptStatus.Verified,
				PerformedBy = "UserMakae",
				Client = client,
				Pallets = pallets,
			};
			DbContext.Clients.Add(client);
			DbContext.Categories.Add(category);
			DbContext.Products.AddRange(product, product1);
			DbContext.Locations.AddRange(location, location1);
			DbContext.Pallets.AddRange(pallets);
			DbContext.Receipts.Add(recipt);
			await DbContext.SaveChangesAsync();

			// Act 1 – create issue with 1 pallet (10 szt.)
			var createIssueDto = new CreateIssueDTO
			{
				ClientId = client.Id,
				PerformedBy = "User1",
				Items = new List<IssueItemDTO>
				{
					new IssueItemDTO { ProductId = product.Id, Quantity = 18, BestBefore = new DateOnly(2026,1,1) },
					new IssueItemDTO { ProductId = product1.Id, Quantity = 4, BestBefore = new DateOnly(2026,1,1) }
				}
			};

			var created = await Mediator.Send(new CreateNewIssueCommand(createIssueDto, DateTime.UtcNow.AddDays(7)));

			var issue = DbContext.Issues.Include(i => i.Pallets).First();
			Assert.Single(issue.Pallets); // powinien być przypisany P1
			Assert.Equal(PalletStatus.InTransit, issue.Pallets.First().Status);
			//Act 2 - wykonanie pickingu
			var pickingFromBase = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product.Id);
			var toPicking = new PickingTaskDTO
			{
				Id = pickingFromBase.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase.BestBefore,
				RequestedQuantity = pickingFromBase.RequestedQuantity,
				PickedQuantity = 8,
				SourcePalletId = "P2",
				ProductId = product.Id,

			};
			var _DoPicking = new DoPlannedPickingCommand(toPicking, "UserPicking");
			var resultPicking = await Mediator.Send(_DoPicking);
			var pickingPallet = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			//Assert 2
			var pickingTaskDone = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product.Id);
			Assert.NotNull(pickingTaskDone);

			//Act 2.1 - wykonanie pickingu
			var pickingFromBase1 = await DbContext.PickingTasks.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product1.Id);
			var toPicking1 = new PickingTaskDTO
			{
				Id = pickingFromBase1.Id,
				PickingStatus = PickingStatus.Allocated,
				BestBefore = pickingFromBase1.BestBefore,
				RequestedQuantity = pickingFromBase1.RequestedQuantity,
				PickedQuantity = 4,
				SourcePalletId = "P4",
				ProductId = product1.Id,

			};
			var _DoPicking1 = new DoPlannedPickingCommand(toPicking1, "UserPicking1");
			var resultPicking1 = await Mediator.Send(_DoPicking1);
			var pickingPallet1 = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0002");
			//Assert 2.1
			var pickingTaskDone1 = await DbContext.PickingTasks
				.FirstOrDefaultAsync(x => x.IssueId == issue.Id && x.ProductId == product1.Id);
			Assert.NotNull(pickingTaskDone1);

			// Act 3 - cancel issue
			var issueToCancelId = issue.Id;
			var result = await Mediator.Send(new CancelIssueCommand(issueToCancelId, "UserC"));
			//Assert
			var cancelledIssue = await DbContext.Issues
				.Include(i => i.Pallets)
				.FirstAsync(i => i.Id == issue.Id);

			Assert.Equal(IssueStatus.Cancelled, cancelledIssue.IssueStatus);
			Assert.Equal("UserC", cancelledIssue.PerformedBy);

			// Assert 3– Pallets restored
			var palletP1 = await DbContext.Pallets.FirstAsync(p => p.Id == "P1");
			Assert.Equal(PalletStatus.Available, palletP1.Status);
			Assert.Null(palletP1.IssueId);
			Assert.Equal(1, palletP1.LocationId);

			var palletP2 = await DbContext.Pallets.FirstAsync(p => p.Id == "P2");
		//	Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Equal(PalletStatus.ToPicking, palletP2.Status);
			Assert.Null(palletP2.IssueId);
			Assert.Equal(1, palletP2.LocationId);
			// Assert 3– No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Equal(2, pickingTasks.Count);

			// Assert 3– Result
			Assert.True(result.IsSuccess);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 wykonanie dekompletacji
			var list = new List<Pallet>();
			var existingPallet = DbContext.Pallets.Find("P3");
			list.Add(existingPallet);
			var resultReversePicking = await Mediator.Send(
				new ExecutiveReversePickingCommand(task.Id, ReversePickingStrategy.AddToExistingPallet,
				"Q0001", "UserReverse", list));
			//Assert 4
			Assert.NotNull(resultReversePicking);
			Assert.True(resultReversePicking.Result.Success);
			Assert.Contains("Dodano towar.", resultReversePicking.Result.Message);
			var palletAfterreversePicking = await DbContext.Pallets.FirstOrDefaultAsync(p => p.Id == "P3");
			Assert.Contains(palletAfterreversePicking, resultReversePicking.Result.PalletWithAddedProduct);
			Assert.NotNull(palletAfterreversePicking);
			Assert.Equal(9, palletAfterreversePicking.ProductsOnPallet.Single().Quantity);
			Assert.Equal(PalletStatus.Available, palletAfterreversePicking.Status);

			var pickingPalletAfterReverse = await DbContext.Pallets.FirstOrDefaultAsync(x => x.Id == "Q0001");
			Assert.NotNull(pickingPalletAfterReverse);
			Assert.Equal(0, pickingPalletAfterReverse.ProductsOnPallet.First(x=>x.ProductId == product.Id).Quantity);
			Assert.Equal(4, pickingPalletAfterReverse.ProductsOnPallet.First(x=>x.ProductId == product1.Id).Quantity);
			Assert.Equal(PalletStatus.ReversePicking, pickingPalletAfterReverse.Status);
		}
	}
}
