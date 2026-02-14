using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Issues.Commands.CancelIssue;
using MyWerehouse.Application.Issues.Commands.CreateNewIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.PickingPallets.Commands.DoPlannedPicking;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Application.Services;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receviving.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.SeviceTests.ReversePickingServiceTests.Integration
{
	public class ReversePickingViewTests : ReverseIntegrationCommandService
	{
		[Fact]
		public async Task GetReverseTasks_PalletsAsignmentAndPickingTaskDone_ShouldReturnList()
		{
			// Arrange – setup initial data
			//var _issueService = new IssueService(Mediator);
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
						//ReceiptId = 1,
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
						//ReceiptId = 1,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			var recipt = new Receipt
			{
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
				.FirstOrDefaultAsync(x => x.Id == 1);
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

			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.Success);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetListReversePickingToDoQuery(1, 1, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.Single(resultGetView.DTOs);
		}
		[Fact]
		public async Task GetReturnReverseTask_PalletsAsignmentAndPickingTaskDone_ShouldReturnInfoThreeOptionsAvailable()
		{
			// Arrange – setup initial data
			//var _issueService = new IssueService(Mediator);
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
						//ReceiptId = 1,
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
						//ReceiptId = 1,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 10, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			var recipt = new Receipt
			{
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
				.FirstOrDefaultAsync(x => x.Id == 1);
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

			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.Success);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.True(resultGetView.CanReturnToSource);
			Assert.True(resultGetView.CanAddToExistingPallet);
			Assert.True(resultGetView.AddToNewPallet);
		}
		[Fact]
		public async Task GetReturnReverseTask_PalletsAsignmentAndPickingTaskDone_ShouldReturnInfoTwoOptionsAvailable()
		{
			// Arrange – setup initial data
			//var _issueService = new IssueService(Mediator);
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
						//ReceiptId = 1,
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
						//ReceiptId = 1,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 8, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P3",
						Location = location,
						Status = PalletStatus.Available,
						//ReceiptId = 1,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 7, BestBefore = new DateOnly(2026,1,1) }
						}
					},
					new Pallet
					{
						Id = "P4",
						Location = location,
						Status = PalletStatus.Available,
						//ReceiptId = 1,
						ProductsOnPallet = new List<ProductOnPallet>
						{
							new ProductOnPallet { Product = product, Quantity = 5, BestBefore = new DateOnly(2026,1,1) }
						}
					}
				};
			var recipt = new Receipt
			{
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
				.FirstOrDefaultAsync(x => x.Id == 1);
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

			// Assert – No pickingTasks left
			var pickingTasks = await DbContext.PickingTasks
				.Where(a => a.IssueId == issue.Id)
				.ToListAsync();

			Assert.Single(pickingTasks);

			// Assert – Result
			Assert.True(result.Success);
			Assert.Contains("Anulowano zlecenie", result.Message);


			var reverseTasks = await DbContext.ReversePickings
				.Where(rp => rp.SourcePalletId == "P2")
				.ToListAsync();

			Assert.Single(reverseTasks);

			var task = reverseTasks.First();
			//Assert.Equal(pickingPallet.Id, task.PickingPalletId);
			Assert.Equal(ReversePickingStatus.Pending, task.Status);
			Assert.Equal("UserC", task.UserId);
			//Act 4 
			var resultGetView = await Mediator.Send(new GetReversePickingToDoQuery(task.Id));
			//Assert 4
			Assert.NotNull(resultGetView);
			Assert.False(resultGetView.CanReturnToSource);
			Assert.True(resultGetView.CanAddToExistingPallet);
			Assert.True(resultGetView.AddToNewPallet);
			Assert.Equal(2, resultGetView.ListPalletsToAdd.Count);
		}		
	}
}
