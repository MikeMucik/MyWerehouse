using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Server.ServicesToInfrastructure;

namespace MyWerehouse.Test.SQLiteInMemoryMode.ReadServiceTests
{
	public class ProductReadServiceTests : TestBase
	{
		private static readonly Guid ProductId =
			Guid.Parse("00000000-0000-0000-0001-000000000000");
		private static readonly Guid HiddenProductId =
			Guid.Parse("00000000-0000-0000-9001-000000000000");

		private readonly ProductReadService _productReadService;

		public ProductReadServiceTests()
		{
			TestDataSeeder.SeedDatabase(DbContext);
			DbContext.ChangeTracker.Clear();
			_productReadService = new ProductReadService(DbContext);
		}

		[Fact]
		public async Task DetailsOfProductAsync_ShouldReturnMappedProduct_WhenProductExists()
		{
			// Act
			var result = await _productReadService.DetailsOfProductAsync(
				ProductId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ProductId, result.Id);
			Assert.Equal("Test", result.Name);
			Assert.Equal(1, result.CategoryId);
			Assert.Equal("TestCategory", result.CategoryName);
			Assert.Equal(56, result.CartonsPerPallet);
			Assert.Equal(10, result.Length);
			Assert.Equal(20, result.Height);
			Assert.Equal(30, result.Width);
			Assert.Equal(2, result.Weight);
			Assert.Equal("TestDetails", result.Description);
		}

		[Fact]
		public async Task DetailsOfProductAsync_ShouldReturnNull_WhenProductDoesNotExist()
		{
			// Act
			var result = await _productReadService.DetailsOfProductAsync(
				Guid.NewGuid(),
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public async Task GetProductsAsync_ShouldReturnOnlyActiveProductsOrderedBySkuAndPaged()
		{
			// Arrange
			DbContext.Products.Add(CreateHiddenProduct());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var firstPage = await _productReadService.GetProductsAsync(
				pageNumber: 1,
				pageSize: 2,
				CancellationToken.None);
			var secondPage = await _productReadService.GetProductsAsync(
				pageNumber: 2,
				pageSize: 2,
				CancellationToken.None);

			// Assert
			Assert.Equal(3, firstPage.TotalCount);
			Assert.Equal(1, firstPage.CurrentPage);
			Assert.Equal(2, firstPage.PageSize);
			Assert.Equal(
				["0987654321", "fghtredfg"],
				firstPage.Items.Select(product => product.SKU));

			Assert.Equal(3, secondPage.TotalCount);
			Assert.Equal(2, secondPage.CurrentPage);
			var lastProduct = Assert.Single(secondPage.Items);
			Assert.Equal("fghtredfg1", lastProduct.SKU);
			Assert.DoesNotContain(
				firstPage.Items.Concat(secondPage.Items),
				product => product.SKU == "000-HIDDEN");
		}

		[Fact]
		public async Task FindProductsByFilterAsync_ShouldApplyProductAndDetailsFilters()
		{
			// Arrange
			var filter = new ProductSearchFilter
			{
				ProductName = "Test",
				SKU = "098",
				Category = "TestCategory",
				CategoryId = 1,
				Length = 10,
				Height = 20,
				Width = 30,
				Weight = 2
			};

			// Act
			var result = await _productReadService.FindProductsByFilterAsync(
				pageNumber: 1,
				pageSize: 10,
				filter,
				CancellationToken.None);

			// Assert
			Assert.Equal(1, result.TotalCount);
			var product = Assert.Single(result.Items);
			Assert.Equal("Test", product.Name);
			Assert.Equal("0987654321", product.SKU);
			Assert.Equal("TestCategory", product.Category);
		}

		[Fact]
		public async Task FindProductsByFilterAsync_ShouldNotReturnHiddenProduct()
		{
			// Arrange
			DbContext.Products.Add(CreateHiddenProduct());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();
			var filter = new ProductSearchFilter
			{
				ProductName = "Hidden"
			};

			// Act
			var result = await _productReadService.FindProductsByFilterAsync(
				pageNumber: 1,
				pageSize: 10,
				filter,
				CancellationToken.None);

			// Assert
			Assert.Equal(0, result.TotalCount);
			Assert.Empty(result.Items);
		}

		[Fact]
		public async Task GetProductToEditAsync_ShouldReturnMappedProduct_WhenActiveProductExists()
		{
			// Act
			var result = await _productReadService.GetProductToEditAsync(
				ProductId,
				CancellationToken.None);

			// Assert
			Assert.NotNull(result);
			Assert.Equal(ProductId, result.Id);
			Assert.Equal("Test", result.Name);
			Assert.Equal("0987654321", result.SKU);
			Assert.Equal(1, result.CategoryId);
			Assert.Equal(56, result.CartonsPerPallet);
			Assert.Equal(10, result.Length);
			Assert.Equal(20, result.Height);
			Assert.Equal(30, result.Width);
			Assert.Equal(2, result.Weight);
			Assert.Equal("TestDetails", result.Description);
		}

		[Fact]
		public async Task GetProductToEditAsync_ShouldReturnNull_WhenProductIsHidden()
		{
			// Arrange
			DbContext.Products.Add(CreateHiddenProduct());
			await DbContext.SaveChangesAsync();
			DbContext.ChangeTracker.Clear();

			// Act
			var result = await _productReadService.GetProductToEditAsync(
				HiddenProductId,
				CancellationToken.None);

			// Assert
			Assert.Null(result);
		}

		private static Product CreateHiddenProduct()
			=> Product.CreateForSeed(
				HiddenProductId,
				"HiddenProduct",
				"000-HIDDEN",
				new DateTime(2025, 5, 1),
				categoryId: 1,
				isDeleted: true,
				cartonsPerPallet: 10,
				length: 10,
				height: 10,
				width: 10,
				weight: 10,
				description: "Hidden product");
	}
}
