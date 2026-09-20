using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.ViewModels.CategoryModels;
using TestSupport.Seeders.Scenarios;

namespace MyWerehouse.Api.Tests.Controllers.Categories
{


	public class CategoryControllerTests
		: IClassFixture<ApiWebApplicationFactory>
	{
		private readonly ApiWebApplicationFactory _factory;

		public CategoryControllerTests(
			ApiWebApplicationFactory factory)
		{
			_factory = factory;
			_factory.ResetAndSeedDatabase(SeederForCategory.SeedDatabase);
		}

		[Fact]
		public async Task GetCategory_ReturnsOk_WhenExist()
		{
			//Arrange
			using var client = _factory.CreateClient();

			//Act
			using var response = await client.GetAsync(
					"/api/categories/1");
			//Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);

			var category = await response.Content
				.ReadFromJsonAsync<CategoryViewDTO>();

			category.Should().NotBeNull();
			category!.Name.Should().Be("TestCategory");
		}
		[Fact]
		public async Task GetById_WhenCategoryDoesNotExist_Returns404()
		{
			// Arrange
			var categoryId = 10000;//not exist
			using var client = _factory.CreateClient();
			// Act
			using var response = await client.GetAsync(
					$"/api/categories/{categoryId}");
			// Assert
			response.StatusCode.Should()
				.Be(HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task GetCategories_ReturnsOk()
		{
			//Arrange
			using var client = _factory.CreateClient();
			//Act
			using var response = await client.GetAsync("/api/categories");
			//Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task AddCategory_ReturnOk()
		{
			//Arrange
			using var client = _factory.CreateClient();
			var categoryDTO = new CategoryDTO { Name = "new CategoryTest" };
			//Act
			using var response = await client.PostAsJsonAsync("/api/categories", categoryDTO, CancellationToken.None);
			//Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);
			var categories = await client
			.GetFromJsonAsync<PagedResult<CategoryViewDTO>>(
				"/api/categories?page=1&size=100");

			categories.Should().NotBeNull();
			categories!.Items.Should()
				.Contain(category => category.Name == categoryDTO.Name);
		}
		[Fact]
		public async Task UpdateCategory_WhenExists_ChangesName()
		{
			// Arrange
			using var client = _factory.CreateClient();
			var categoryId = 1;

			var dto = new CategoryDTO
			{
				Name = "Changed Name"
			};

			// Act
			using var response = await client.PutAsJsonAsync(
				$"/api/categories/{categoryId}", dto);

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);

			var category = await client.GetFromJsonAsync<CategoryViewDTO>(
				$"/api/categories/{categoryId}");

			category.Should().NotBeNull();
			category!.Name.Should().Be(dto.Name);
		}
		[Fact]
		public async Task DeleteCategory_WhenExistsWithoutProducts_RemovesCategory()
		{
			// Arrange
			using var client = _factory.CreateClient();
			var categoryId = 3;

			// Act
			using var response = await client.DeleteAsync(
				$"/api/categories/{categoryId}");

			// Assert
			response.StatusCode.Should().Be(HttpStatusCode.OK);

			using var getResponse = await client.GetAsync(
				$"/api/categories/{categoryId}");

			getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}
	}
}

