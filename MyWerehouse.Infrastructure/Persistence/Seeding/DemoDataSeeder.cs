using Microsoft.EntityFrameworkCore;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Common.ValueObject;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Inventories.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Infrastructure.Persistence.Seeding
{
	public static class DemoDataSeeder
	{
		private const string MarkerSku = "DEMO-COF-001";
		private const string DemoUser = "demo.user";

		public static async Task SeedAsync(WerehouseDbContext dbContext, CancellationToken ct = default)
		{
			var strategy = dbContext.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				dbContext.ChangeTracker.Clear();
				if (await dbContext.Products.AnyAsync(p => p.SKU == MarkerSku, ct))
					return;

				await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);
				var now = DateTime.UtcNow;

				var food = new Category { Name = "Demo Food" };
				var household = new Category { Name = "Demo Household" };
				var retailClient = CreateClient(
					"Demo Retail",
					"Demo Retail Company",
					"demo.retail@example.com",
					"Warsaw",
					"00-001",
					"Warehouse Street",
					"10");
				var marketClient = CreateClient(
					"Demo Market",
					"Demo Market Company",
					"demo.market@example.com",
					"Krakow",
					"30-001",
					"Market Street",
					"20");

				var locations = Enumerable.Range(1, 9)
					.Select(position => new Location
					{
						Aisle = 1,
						Bay = 1,
						Position = position,
						Height = 1
					})
					.ToList();
				var demoRamp = new Location { Aisle = 9, Bay = 9, Position = 1, Height = 1 };

				dbContext.Categories.AddRange(food, household);
				dbContext.Clients.AddRange(retailClient, marketClient);
				dbContext.Locations.AddRange(locations.Append(demoRamp));
				await dbContext.SaveChangesAsync(ct);

				var coffee = Product.CreateForSeed(
					Guid.Parse("10000000-0000-0000-0000-000000000001"),
					"Demo Coffee", MarkerSku, now.AddDays(-60), food.Id, false, 20,
					40, 30, 30, 8000, "Coffee used in the planned-picking demo.");
				var tea = Product.CreateForSeed(
					Guid.Parse("10000000-0000-0000-0000-000000000002"),
					"Demo Tea", "DEMO-TEA-001", now.AddDays(-45), food.Id, false, 15,
					40, 30, 30, 6000, "Tea used in the manual-picking demo.");
				var pasta = Product.CreateForSeed(
					Guid.Parse("10000000-0000-0000-0000-000000000003"),
					"Demo Pasta", "DEMO-PAS-001", now.AddDays(-30), food.Id, false, 24,
					50, 35, 30, 10000, "Pasta assigned to an issue ready for loading.");
				var detergent = Product.CreateForSeed(
					Guid.Parse("10000000-0000-0000-0000-000000000004"),
					"Demo Detergent", "DEMO-DET-001", now.AddDays(-20), household.Id, false, 18,
					45, 40, 35, 12000, "Product without a best-before date.");

				var plannedIssue = CreateIssue(
					Guid.Parse("20000000-0000-0000-0000-000000000001"), 900001,
					retailClient.Id, now, 3, IssueStatus.Pending);
				plannedIssue.AddIssueItem(coffee.Id, 28, DateOnly.FromDateTime(now.AddDays(90)), now);

				var manualIssue = CreateIssue(
					Guid.Parse("20000000-0000-0000-0000-000000000002"), 900002,
					marketClient.Id, now, 2, IssueStatus.InProgress);
				manualIssue.AddIssueItem(tea.Id, 12, DateOnly.FromDateTime(now.AddDays(60)), now);

				var loadingIssue = CreateIssue(
					Guid.Parse("20000000-0000-0000-0000-000000000003"), 900003,
					retailClient.Id, now, 1, IssueStatus.ConfirmedToLoad);
				loadingIssue.AddIssueItem(pasta.Id, 24, DateOnly.FromDateTime(now.AddDays(120)), now);

				var reverseIssue = CreateIssue(
					Guid.Parse("20000000-0000-0000-0000-000000000004"), 900004,
					marketClient.Id, now.AddDays(-2), 1, IssueStatus.Cancelled);
				reverseIssue.AddIssueItem(coffee.Id, 5, DateOnly.FromDateTime(now.AddDays(90)), now.AddDays(-2));

				var receipt = Receipt.CreateForSeed(
					Guid.Parse("70000000-0000-0000-0000-000000000001"),
					900001, retailClient.Id, DemoUser, now.AddDays(-7), ReceiptStatus.Verified, demoRamp.Id);

				var coffeeBb1 = DateOnly.FromDateTime(now.AddDays(180));
				var coffeeBb2 = DateOnly.FromDateTime(now.AddDays(240));
				var teaBb1 = DateOnly.FromDateTime(now.AddDays(120));
				var teaBb2 = DateOnly.FromDateTime(now.AddDays(180));
				var pastaBb = DateOnly.FromDateTime(now.AddDays(365));

				var fullCoffee = CreatePallet("30000000-0000-0000-0000-000000000001", "DEMO-P001",
					now, locations[0].Id, PalletStatus.LockedForIssue, receipt.Id, plannedIssue.Id, coffee.Id, 20, coffeeBb1);
				var plannedSource = CreatePallet("30000000-0000-0000-0000-000000000002", "DEMO-P002",
					now, locations[1].Id, PalletStatus.ToPicking, receipt.Id, null, coffee.Id, 20, coffeeBb2);
				var teaPallet1 = CreatePallet("30000000-0000-0000-0000-000000000003", "DEMO-P003",
					now, locations[2].Id, PalletStatus.Available, receipt.Id, null, tea.Id, 15, teaBb1);
				var teaPallet2 = CreatePallet("30000000-0000-0000-0000-000000000004", "DEMO-P004",
					now, locations[3].Id, PalletStatus.Available, receipt.Id, null, tea.Id, 15, teaBb2);
				var loadingPallet = CreatePallet("30000000-0000-0000-0000-000000000005", "DEMO-P005",
					now, locations[4].Id, PalletStatus.ToIssue, receipt.Id, loadingIssue.Id, pasta.Id, 24, pastaBb);
				var detergentPallet = CreatePallet("30000000-0000-0000-0000-000000000006", "DEMO-P006",
					now, locations[5].Id, PalletStatus.Available, receipt.Id, null, detergent.Id, 18, null);
				var reverseSource = CreatePallet("30000000-0000-0000-0000-000000000007", "DEMO-P007",
					now, locations[6].Id, PalletStatus.ToPicking, receipt.Id, null, coffee.Id, 15, coffeeBb1);
				var reversePickingPallet = CreatePallet("30000000-0000-0000-0000-000000000008", "DEMO-Q001",
					now, locations[7].Id, PalletStatus.ReversePicking, null, reverseIssue.Id, coffee.Id, 5, coffeeBb1);

				var plannedVirtualPallet = VirtualPallet.CreateForSeed(
					Guid.Parse("40000000-0000-0000-0000-000000000001"),
					plannedSource.Id, 20, plannedSource.LocationId, now.AddDays(-1));
				var reverseVirtualPallet = VirtualPallet.CreateForSeed(
					Guid.Parse("40000000-0000-0000-0000-000000000002"),
					reverseSource.Id, 20, reverseSource.LocationId, now.AddDays(-2));

				var plannedTask = PickingTask.CreateForSeed(
					Guid.Parse("50000000-0000-0000-0000-000000000001"),
					plannedVirtualPallet.Id, plannedIssue.Id, 8, PickingStatus.Allocated, coffee.Id,
					DateOnly.FromDateTime(now.AddDays(90)), null, DateOnly.FromDateTime(now.AddDays(1)), 0);
				var manualTask = PickingTask.CreateForSeed(
					Guid.Parse("50000000-0000-0000-0000-000000000002"),
					null, manualIssue.Id, 12, PickingStatus.Available, tea.Id,
					DateOnly.FromDateTime(now.AddDays(60)), null, DateOnly.FromDateTime(now), 0);
				var pickedTask = PickingTask.CreateForSeed(
					Guid.Parse("50000000-0000-0000-0000-000000000003"),
					reverseVirtualPallet.Id, reverseIssue.Id, 5, PickingStatus.Picked, coffee.Id,
					DateOnly.FromDateTime(now.AddDays(90)), reversePickingPallet.Id, DateOnly.FromDateTime(now.AddDays(-2)), 5);

				var reverseTask = ReversePickingTask.CreateForSeed(
					Guid.Parse("60000000-0000-0000-0000-000000000001"),
					reversePickingPallet.Id, reverseSource.Id, coffee.Id, coffeeBb1, 5,
					pickedTask.Id, DemoUser, DateOnly.FromDateTime(now));

				dbContext.Products.AddRange(coffee, tea, pasta, detergent);
				dbContext.Inventories.AddRange(
					Inventory.CreateStockItem(coffee.Id, 60, now),
					Inventory.CreateStockItem(tea.Id, 30, now),
					Inventory.CreateStockItem(pasta.Id, 24, now),
					Inventory.CreateStockItem(detergent.Id, 18, now));
				dbContext.Receipts.Add(receipt);
				dbContext.Issues.AddRange(plannedIssue, manualIssue, loadingIssue, reverseIssue);
				dbContext.Pallets.AddRange(
					fullCoffee, plannedSource, teaPallet1, teaPallet2,
					loadingPallet, detergentPallet, reverseSource, reversePickingPallet);
				dbContext.VirtualPallets.AddRange(plannedVirtualPallet, reverseVirtualPallet);
				dbContext.PickingTasks.AddRange(plannedTask, manualTask, pickedTask);
				dbContext.ReversePickings.Add(reverseTask);

				AddDemoHistory(
					dbContext, now, receipt,
					new[] { fullCoffee, plannedSource, teaPallet1, teaPallet2, loadingPallet, detergentPallet, reverseSource },
					new[] { plannedIssue, manualIssue, loadingIssue, reverseIssue },
					new[] { plannedTask, manualTask, pickedTask },
					new Dictionary<Guid, (Guid? PalletId, string? PalletNumber)>
					{
						[plannedTask.Id] = (plannedSource.Id, plannedSource.PalletNumber),
						[manualTask.Id] = (null, null),
						[pickedTask.Id] = (reverseSource.Id, reverseSource.PalletNumber)
					});

				await dbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
			});
		}

		private static Client CreateClient(
			string name, string fullName, string email,
			string city, string postalCode, string street, string streetNumber)
		{
			return new Client
			{
				Name = name,
				FullName = fullName,
				Email = email,
				Description = "Fictional client created for the public API demo.",
				Addresses = new List<Address>
				{
					new Address
					{
						Country = "Poland",
						City = city,
						Region = "Demo Region",
						Phone = 481234567,
						PostalCode = postalCode,
						StreetName = street,
						StreetNumber = streetNumber
					}
				}
			};
		}

		private static Issue CreateIssue(
			Guid id, int number, int clientId, DateTime createdAt, int sendInDays, IssueStatus status)
		{
			return Issue.CreateForSeed(
				id, number, clientId, createdAt,
				DateOnly.FromDateTime(createdAt.AddDays(sendInDays)),
				DemoUser, status, null);
		}

		private static Pallet CreatePallet(
			string id, string number, DateTime now, int locationId, PalletStatus status,
			Guid? receiptId, Guid? issueId, Guid productId, int quantity, DateOnly? bestBefore)
		{
			var pallet = Pallet.CreateForSeed(
				Guid.Parse(id), number, now.AddDays(-7), locationId, status, receiptId, issueId);
			pallet.AddProduct(productId, quantity, now.AddDays(-7), bestBefore);
			return pallet;
		}

		private static void AddDemoHistory(
			WerehouseDbContext dbContext,
			DateTime now,
			Receipt receipt,
			IReadOnlyCollection<Pallet> receiptPallets,
			IReadOnlyCollection<Issue> issues,
			IReadOnlyCollection<PickingTask> pickingTasks,
			IReadOnlyDictionary<Guid, (Guid? PalletId, string? PalletNumber)> pickingSources)
		{
			var receiptHistory = new HistoryReceipt
			{
				ReceiptId = receipt.Id,
				ReceiptNumber = receipt.ReceiptNumber,
				ClientId = receipt.ClientId,
				StatusAfter = ReceiptStatus.Verified,
				PerformedBy = DemoUser,
				DateTime = now.AddDays(-6),
				Details = receiptPallets.Select(p => new HistoryReceiptDetail
				{
					PalletId = p.Id,
					PalletNumber = p.PalletNumber,
					LocationId = p.LocationId,
					LocationSnapShot = $"Demo location {p.LocationId}"
				}).ToList()
			};

			dbContext.HistoryReceipts.Add(receiptHistory);
			dbContext.HistoryIssues.AddRange(issues.Select(issue => new HistoryIssue
			{
				IssueId = issue.Id,
				IssueNumber = issue.IssueNumber,
				ClientId = issue.ClientId,
				StatusAfter = issue.IssueStatus,
				PerformedBy = DemoUser,
				DateTime = now.AddDays(-1),
				Items = issue.IssueItems.Select(item => new HistoryIssueItems
				{
					ProductId = item.ProductId,
					Quantity = item.Quantity,
					BestBefore = item.BestBefore
				}).ToList()
			}));
			dbContext.HistoryPickings.AddRange(pickingTasks.Select(task =>
			{
				var source = pickingSources[task.Id];
				return new HistoryPicking
				{
					PickingTaskId = task.Id,
					PalletId = source.PalletId,
					PalletNumber = source.PalletNumber,
					PickingPalletId = task.PickingPalletId,
					IssueId = task.IssueId,
					IssueNumber = issues.Single(i => i.Id == task.IssueId).IssueNumber,
					ProductId = task.ProductId,
					QuantityAllocated = task.RequestedQuantity,
					QuantityPicked = task.PickedQuantity,
					StatusBefore = task.PickingStatus == PickingStatus.Picked
						? PickingStatus.Allocated
						: PickingStatus.Available,
					StatusAfter = task.PickingStatus,
					PerformedBy = DemoUser,
					DateTime = now.AddDays(-1)
				};
			}));

			foreach (var pallet in receiptPallets)
			{
				dbContext.HistoryPallet.Add(new HistoryPallet
				{
					PalletId = pallet.Id,
					PalletNumber = pallet.PalletNumber,
					DestinationLocationId = pallet.LocationId,
					DestinationLocationSnapShot = $"Demo location {pallet.LocationId}",
					Reason = ReasonForPallet.Received,
					PerformedBy = DemoUser,
					MovementDate = now.AddDays(-6),
					PalletStatus = pallet.Status,
					HistoryPalletDetails = pallet.ProductsOnPallet
						.Select(product => new HistoryPalletDetail(product.ProductId, product.Quantity))
						.ToList()
				});
			}
		}
	}
}
