using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/products")]
	public class ProductsController : ControllerBase
	{
		private readonly IProductService _productService;
		public ProductsController(IProductService productService)
		{
			_productService = productService;
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateProductDTO productDto, CancellationToken ct)
			=> (await _productService.AddProductAsync(productDto, ct))
			.ToActionResult();

		[HttpGet("{id:guid}/edit")]
		public async Task<IActionResult> GetForEdit(Guid id, CancellationToken ct)
			=> (await _productService.GetProductToEditAsync(id, ct))
			.ToActionResult();

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, EditProductDTO productDto, CancellationToken ct)
			=> (await _productService.UpdateProductAsync(id, productDto, ct))
			.ToActionResult();

		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
			=> (await _productService.DeleteProductAsync(id, ct))
			.ToActionResult();

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
			=> (await _productService.DetailsOfProductAsync(id, ct))
			.ToActionResult();

		[HttpGet]
		public async Task<IActionResult> GetAll(
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			CancellationToken ct = default)
			=> (await _productService.GetProductsAsync(pageNumber, pageSize, ct))
			.ToActionResult();

		[HttpGet("search")]
		public async Task<IActionResult> Search(
			[FromQuery] ProductSearchFilter filter,
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			CancellationToken ct = default)
			=> (await _productService.FindProductsByFilterAsync(pageNumber, pageSize, filter, ct))
			.ToActionResult();
	}
}
