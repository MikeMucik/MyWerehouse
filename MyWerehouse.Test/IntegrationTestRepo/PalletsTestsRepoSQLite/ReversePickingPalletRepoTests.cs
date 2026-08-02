using MyWerehouse.Domain.Clients.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Receiving.Models;
using MyWerehouse.Domain.Warehouse.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.PalletsTestsRepoSQLite
{
    public class ReversePickingPalletRepoTests : TestBase
    {
        [Fact]
        public async Task GetAvailablePalletsForReversePicking_ShouldReturnOnlyMatchingPallets()
        {
            var client = CreateClient();
            var category = new Category { Name = "Category", IsDeleted = false };
            var locations = Enumerable.Range(1, 7)
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
                category.Id, 10, 30, 30, 30, 30, "Details");
            var receipt = Receipt.CreateForSeed(Guid.NewGuid(), 1, client.Id, "user",
                TestDates.UtcNow, ReceiptStatus.Verified, locations[0].Id);
            DbContext.Products.Add(product);
            DbContext.Receipts.Add(receipt);
            await DbContext.SaveChangesAsync();

            var requiredBestBefore = new DateOnly(2027, 1, 10);
            var source = CreatePallet("SOURCE", locations[0].Id, PalletStatus.Available,
                receipt.Id, product.Id, 2, requiredBestBefore);
            var matching = CreatePallet("MATCH", locations[1].Id, PalletStatus.Available,
                receipt.Id, product.Id, 7, requiredBestBefore);
            var wrongBestBefore = CreatePallet("WRONG-BB", locations[2].Id, PalletStatus.Available,
                receipt.Id, product.Id, 5, requiredBestBefore.AddDays(1));
            var full = CreatePallet("FULL", locations[3].Id, PalletStatus.Available,
                receipt.Id, product.Id, 10, requiredBestBefore);
            var multipleLines = CreatePallet("MULTI", locations[4].Id, PalletStatus.Available,
                receipt.Id, product.Id, 3, requiredBestBefore);
            multipleLines.AddProduct(product.Id, 1, TestDates.UtcNow, requiredBestBefore.AddDays(1));
            var withoutReceipt = CreatePallet("NO-RECEIPT", locations[5].Id, PalletStatus.Available,
                null, product.Id, 4, requiredBestBefore);
            var wrongStatus = CreatePallet("WRONG-STATUS", locations[6].Id, PalletStatus.ToPicking,
                receipt.Id, product.Id, 4, requiredBestBefore);
            DbContext.Pallets.AddRange(source, matching, wrongBestBefore, full,
                multipleLines, withoutReceipt, wrongStatus);
            await DbContext.SaveChangesAsync();
            DbContext.ChangeTracker.Clear();

            var repo = new PalletRepo(DbContext);
            var result = await repo.GetAvailablePalletsForReversePickingAsync(
                product.Id, requiredBestBefore, source.Id, product.CartonsPerPallet);

            var pallet = Assert.Single(result);
            Assert.Equal(matching.Id, pallet.Id);
            Assert.NotNull(pallet.Location);
            Assert.Single(pallet.ProductsOnPallet);
        }

        private static Pallet CreatePallet(string number, int locationId, PalletStatus status,
            Guid? receiptId, Guid productId, int quantity, DateOnly? bestBefore)
        {
            var pallet = Pallet.CreateForTests(number, TestDates.UtcNow, locationId,
                status, receiptId, null);
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
