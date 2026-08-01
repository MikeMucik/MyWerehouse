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
		public async Task<IActionResult> Create(CreateProductDTO productDto)
			=> (await _productService.AddProductAsync(productDto))
			.ToActionResult();

		[HttpGet("{id:guid}/edit")]
		public async Task<IActionResult> GetForEdit(Guid id)
			=> (await _productService.GetProductToEditAsync(id))
			.ToActionResult();

		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, EditProductDTO productDto)
			=> (await _productService.UpdateProductAsync(id, productDto))
			.ToActionResult();
		
		[HttpDelete("{id:guid}")]
		public async Task<IActionResult> Delete(Guid id)
			=> (await _productService.DeleteProductAsync(id))
			.ToActionResult();

		[HttpGet("{id:guid}")]
		public async Task<IActionResult> GetById(Guid id)
			=> (await _productService.DetailsOfProductAsync(id))
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
