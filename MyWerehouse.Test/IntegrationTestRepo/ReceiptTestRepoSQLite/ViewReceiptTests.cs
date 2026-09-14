using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ReceiptTestRepoSQLite
{
	public class ViewReceiptTests : TestBase
	{
		private static readonly Guid ReceiptId =
			Guid.Parse("11111111-1111-1111-1111-111111111111");
		private static readonly Guid ProductId =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid ProductWithoutReceiptId =
			Guid.Parse("00000000-0000-0000-0989-000000000000");

		private readonly ReceiptRepo _receiptRepo;

		public ViewReceiptTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_receiptRepo = new ReceiptRepo(DbContext);
		}

		[Fact]
		public async Task GetReceiptByIdAsync_ShouldReturnReceiptWithPalletData_WhenReceiptExists()
		{
			// Act
			var result = await _receiptRepo.GetReceiptByIdAsync(
				ReceiptId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ReceiptId, result.Id);
			Assert.Equal(10, result.ClientId);
			Assert.Equal(2, result.Pallets.Count);

			var pallet = result.Pallets.Single(p => p.PalletNumber == "Q1000");
			Assert.NotNull(pallet.Location);
			Assert.Equal(1, pallet.LocationId);
			Assert.Equal(2, pallet.ProductsOnPallet.Count);
			Assert.Contains(
				pallet.ProductsOnPallet,
				product => product.ProductId == ProductId && product.Quantity == 50);
		}

		[Fact]
		public async Task GetReceiptByIdAsync_ShouldReturnNull_WhenReceiptDoesNotExist()
		{
			// Act
			var result = await _receiptRepo.GetReceiptByIdAsync(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetReceipForCancelByIdAsync_ShouldReturnReceiptWithRequiredPalletData()
		{
			// Act
			var result = await _receiptRepo.GetReceipForCancelByIdAsync(
				ReceiptId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(2, result.Pallets.Count);
			Assert.All(result.Pallets, pallet => Assert.NotNull(pallet.Location));

			var pallet = result.Pallets.Single(p => p.PalletNumber == "Q1000");
			Assert.NotEmpty(pallet.PalletHistory);
		}

		[Fact]
		public async Task GetReceipForCancelByIdAsync_ShouldReturnNull_WhenReceiptDoesNotExist()
		{
			// Act
			var result = await _receiptRepo.GetReceipForCancelByIdAsync(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetNextNumberOfReceipt_ShouldReturnNumberAfterCurrentMaximum()
		{
			// Act
			var result = await _receiptRepo.GetNextNumberOfReceipt(
				CancellationToken.None);

			// Assert
			Assert.Equal(3, result);
		}

		[Fact]
		public async Task HasReceiptClient_ShouldReturnExpectedResult()
		{
			// Act
			var resultForUsedClient = await _receiptRepo.HasReceiptClient(
				clientId: 10,
				CancellationToken.None);
			var resultForUnusedClient = await _receiptRepo.HasReceiptClient(
				clientId: 989,
				CancellationToken.None);

			// Assert
			Assert.True(resultForUsedClient);
			Assert.False(resultForUnusedClient);
		}

		[Fact]
		public async Task HasReceiptProduct_ShouldReturnExpectedResult()
		{
			// Act
			var resultForUsedProduct = await _receiptRepo.HasReceiptProduct(
				ProductId,
				CancellationToken.None);
			var resultForUnusedProduct = await _receiptRepo.HasReceiptProduct(
				ProductWithoutReceiptId,
				CancellationToken.None);

			// Assert
			Assert.True(resultForUsedProduct);
			Assert.False(resultForUnusedProduct);
		}
	}
}
