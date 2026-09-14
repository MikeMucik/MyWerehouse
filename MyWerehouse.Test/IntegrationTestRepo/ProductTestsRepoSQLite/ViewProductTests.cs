using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Infrastructure.Persistence.Repositories;
using MyWerehouse.Test.SQLiteInMemoryMode;

namespace MyWerehouse.Test.IntegrationTestRepo.ProductTestsRepoSQLite
{
	public class ViewProductTests : TestBase
	{
		private static readonly Guid ProductId =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid SecondProductId =
			Guid.Parse("00000000-0000-0000-0002-000000000000");
		private static readonly Guid HiddenProductId =
			Guid.Parse("00000000-0000-0000-9002-000000000000");

		private readonly ProductRepo _productRepo;

		public ViewProductTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_productRepo = new ProductRepo(DbContext);
		}

		[Fact]
		public async Task GetProductByIdAsync_ShouldReturnActiveProduct_WhenProductExists()
		{
			// Act
			var result = await _productRepo.GetProductByIdAsync(
				ProductId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ProductId, result.Id);
			Assert.Equal("Test", result.Name);
		}

		[Fact]
		public async Task GetProductByIdAsync_ShouldReturnNull_WhenIdIsInvalid()
		{
			// Act
			var resultForEmptyId = await _productRepo.GetProductByIdAsync(
				Guid.Empty,
				CancellationToken.None);
			var resultForMissingId = await _productRepo.GetProductByIdAsync(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(resultForEmptyId);
			Assert.Null(resultForMissingId);
		}

		[Fact]
		public async Task GetSKUForProductAsync_ShouldReturnSkuOfRequestedProduct()
		{
			// Act
			var firstSku = await _productRepo.GetSKUForProductAsync(
				ProductId,
				CancellationToken.None);
			var secondSku = await _productRepo.GetSKUForProductAsync(
				SecondProductId,
				CancellationToken.None);
			var missingSku = await _productRepo.GetSKUForProductAsync(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Equal("0987654321", firstSku);
			Assert.Equal("fghtredfg", secondSku);
			Assert.Null(missingSku);
		}

		[Fact]
		public async Task GetProductToEditAsync_ShouldReturnProductWithDetails_WhenProductExists()
		{
			// Act
			var result = await _productRepo.GetProductToEditAsync(
				ProductId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ProductId, result.Id);
			Assert.NotNull(result.Details);
			Assert.Equal(10, result.Details.Length);
			Assert.Equal(20, result.Details.Height);
			Assert.Equal(30, result.Details.Width);
			Assert.Equal(2, result.Details.Weight);
		}

		[Fact]
		public async Task AlreadyExist_ShouldCheckBothNameAndSku()
		{
			// Act
			var resultForName = await _productRepo.AlreadyExist("Test", "missing-sku");
			var resultForSku = await _productRepo.AlreadyExist("missing-name", "0987654321");
			var resultForMissingProduct = await _productRepo.AlreadyExist(
				"missing-name",
				"missing-sku");

			// Assert
			Assert.True(resultForName);
			Assert.True(resultForSku);
			Assert.False(resultForMissingProduct);
		}

		[Fact]
		public async Task IsExistProduct_ShouldReturnExpectedResult()
		{
			// Act
			var resultForExistingProduct = await _productRepo.IsExistProduct(
				ProductId,
				CancellationToken.None);
			var resultForMissingProduct = await _productRepo.IsExistProduct(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.True(resultForExistingProduct);
			Assert.False(resultForMissingProduct);
		}

		[Fact]
		public async Task HasProductsInCategory_ShouldReturnExpectedResult()
		{
			// Act
			var resultForUsedCategory = await _productRepo.HasProductsInCategory(
				categoryId: 1,
				CancellationToken.None);
			var resultForEmptyCategory = await _productRepo.HasProductsInCategory(
				categoryId: 2,
				CancellationToken.None);

			// Assert
			Assert.True(resultForUsedCategory);
			Assert.False(resultForEmptyCategory);
		}

		[Fact]
		public async Task ProductReadMethods_ShouldIgnoreHiddenProduct()
		{
			// Arrange
			DbContext.Products.Add(Product.CreateForSeed(
				HiddenProductId,
				"HiddenRepoProduct",
				"HIDDEN-REPO",
				new DateTime(2025, 5, 1),
				categoryId: 1,
				isDeleted: true,
				cartonsPerPallet: 10,
				length: 10,
				height: 10,
				width: 10,
				weight: 10,
				description: "Hidden repository product"));
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var product = await _productRepo.GetProductByIdAsync(
				HiddenProductId,
				CancellationToken.None);
			var sku = await _productRepo.GetSKUForProductAsync(
				HiddenProductId,
				CancellationToken.None);
			var productToEdit = await _productRepo.GetProductToEditAsync(
				HiddenProductId,
				CancellationToken.None);
			var exists = await _productRepo.IsExistProduct(
				HiddenProductId,
				CancellationToken.None);

			// Assert
			Assert.Null(product);
			Assert.Null(sku);
			Assert.Null(productToEdit);
			Assert.False(exists);
		}
	}
}
