using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking;
using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.ReversePickings.Models;
using MyWerehouse.Domain.Warehouse.Models;

namespace MyWerehouse.Test.SQLiteInMemoryMode.HandlersTests.ReversePickingTests.Integration
{
    public class ReversePickingRegressionTests : TestBase
    {
        [Fact]
        public async Task ExecuteReversePicking_ShouldReduceSharedBatchUntilAllTasksAreCompleted()
        {
            var client = CreateClient();
            var category = new Category { Name = "Category", IsDeleted = false };
            var locations = Enumerable.Range(1, 4)
                .Select(id => new Location
                {
                    Id = id,
                    Aisle = 1,
                    Bay = 1,
                    Height = 1,
                    Position = id
                })
                .ToList();
            DbContext.Clients.Add(client);
            DbContext.Categories.Add(category);
            DbContext.Locations.AddRange(locations);
            await DbContext.SaveChangesAsync();

            var product = Product.Create("Product", "SKU", TestDates.UtcNow,
                category.Id, 20, 30, 30, 30, 30, "Details");
            var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, client.Id, "user",
                TestDates.UtcNow, ReceiptStatus.Verified, locations[3].Id);
            var issue = Issue.CreateForSeed(Guid.NewGuid(), 1, client.Id, TestDates.UtcNow,
                DateOnly.FromDateTime(TestDates.UtcNow.AddDays(7)), "user", IssueStatus.Cancelled, null);
            var bestBefore = new DateOnly(2027, 1, 10);
            var firstSource = CreatePallet("SOURCE-1", locations[0].Id, PalletStatus.ToPicking,
                receipt.Id, null, product.Id, 6, bestBefore);
            var secondSource = CreatePallet("SOURCE-2", locations[1].Id, PalletStatus.ToPicking,
                receipt.Id, null, product.Id, 4, bestBefore);
            var pickingPallet = CreatePallet("PICKING", locations[2].Id, PalletStatus.Picking,
                null, issue.Id, product.Id, 10, bestBefore);
            var targetPallet = CreatePallet("TARGET", locations[3].Id, PalletStatus.Available,
                receipt.Id, null, product.Id, 2, bestBefore);
            DbContext.Products.Add(product);
            DbContext.Receipts.Add(receipt);
            DbContext.Issues.Add(issue);
            DbContext.Pallets.AddRange(firstSource, secondSource, pickingPallet, targetPallet);
            await DbContext.SaveChangesAsync();

            var firstVirtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), firstSource.Id,
                10, firstSource.LocationId, TestDates.UtcNow.AddDays(-1));
            var secondVirtualPallet = VirtualPallet.CreateForSeed(Guid.NewGuid(), secondSource.Id,
                10, secondSource.LocationId, TestDates.UtcNow.AddDays(-1));
            var firstPickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), firstVirtualPallet.Id,
                issue.Id, 4, PickingStatus.Picked, product.Id, bestBefore,
                pickingPallet.Id, TestDates.Today, 4);
            var secondPickingTask = PickingTask.CreateForSeed(Guid.NewGuid(), secondVirtualPallet.Id,
                issue.Id, 6, PickingStatus.Picked, product.Id, bestBefore,
                pickingPallet.Id, TestDates.Today, 6);
            var firstReverseTask = ReversePickingTask.Create(pickingPallet.Id, firstSource.Id,
                product.Id, bestBefore, 4, firstPickingTask.Id, "user", TestDates.Today);
            var secondReverseTask = ReversePickingTask.Create(pickingPallet.Id, secondSource.Id,
                product.Id, bestBefore, 6, secondPickingTask.Id, "user", TestDates.Today);
            DbContext.VirtualPallets.AddRange(firstVirtualPallet, secondVirtualPallet);
            DbContext.PickingTasks.AddRange(firstPickingTask, secondPickingTask);
            DbContext.ReversePickings.AddRange(firstReverseTask, secondReverseTask);
            await DbContext.SaveChangesAsync();
            DbContext.ChangeTracker.Clear();

            var firstResult = await Mediator.Send(new ExecuteReversePickingCommand(
                firstReverseTask.Id, ReversePickingStrategy.AddToExistingPallet,
                pickingPallet.Id, "user", [targetPallet.Id], null));

            Assert.True(firstResult.IsSuccess);
            var pickingAfterFirstTask = await DbContext.Pallets
                .AsNoTracking()
                .Include(p => p.ProductsOnPallet)
                .SingleAsync(p => p.Id == pickingPallet.Id);
            var targetAfterFirstTask = await DbContext.Pallets
                .AsNoTracking()
                .Include(p => p.ProductsOnPallet)
                .SingleAsync(p => p.Id == targetPallet.Id);
            Assert.Equal(6, pickingAfterFirstTask.ProductsOnPallet.Single().Quantity);
            Assert.Equal(PalletStatus.ReversePicking, pickingAfterFirstTask.Status);
            Assert.Equal(6, targetAfterFirstTask.ProductsOnPallet.Single().Quantity);

            DbContext.ChangeTracker.Clear();
            var secondResult = await Mediator.Send(new ExecuteReversePickingCommand(
                secondReverseTask.Id, ReversePickingStrategy.AddToExistingPallet,
                pickingPallet.Id, "user", [targetPallet.Id], null));

            Assert.True(secondResult.IsSuccess);
            DbContext.ChangeTracker.Clear();
            var pickingAfterSecondTask = await DbContext.Pallets
                .AsNoTracking()
                .Include(p => p.ProductsOnPallet)
                .SingleAsync(p => p.Id == pickingPallet.Id);
            var targetAfterSecondTask = await DbContext.Pallets
                .AsNoTracking()
                .Include(p => p.ProductsOnPallet)
                .SingleAsync(p => p.Id == targetPallet.Id);
            var reverseTasks = await DbContext.ReversePickings.AsNoTracking().ToListAsync();
            Assert.Equal(0, pickingAfterSecondTask.ProductsOnPallet.Single().Quantity);
            Assert.Equal(PalletStatus.Archived, pickingAfterSecondTask.Status);
            Assert.Equal(12, targetAfterSecondTask.ProductsOnPallet.Single().Quantity);
            Assert.All(reverseTasks, task => Assert.Equal(ReversePickingStatus.Completed, task.Status));
        }

        private static Pallet CreatePallet(string number, int locationId, PalletStatus status,
            Guid? receiptId, Guid? issueId, Guid productId, int quantity, DateOnly? bestBefore)
        {
            var pallet = Pallet.CreateForTests(number, TestDates.UtcNow, locationId,
                status, receiptId, issueId);
            pallet.AddProduct(productId, quantity, TestDates.UtcNow, bestBefore);
            return pallet;
        }

        private static Client CreateClient()
        {
            return new Client
            {
                Name = "Client",
                Email = "client@example.com",
                Description = "Description",
                FullName = "Test Client",
                Addresses =
                [
                    new Address
                    {
                        City = "Warsaw",
                        Country = "Poland",
                        PostalCode = "00-001",
                        StreetName = "Test",
                        Phone = 123456789,
                        Region = "Mazowieckie",
                        StreetNumber = "1"
                    }
                ]
            };
        }
    }
}
