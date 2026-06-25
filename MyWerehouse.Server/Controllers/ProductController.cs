using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Filters;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/product")]
	public class ProductController : ControllerBase
	{
		private readonly IProductService _productService;
		public ProductController(IProductService productService)
		{
			_productService = productService;
		}
		[HttpGet("{id:guid}toEdit")]
		public async Task<IActionResult> GetFullInfo(Guid id)
		{
			var result = await _productService.GetProductToEditAsync(id);
			return Ok(result);
		}
		[HttpPost("add")]
		public async Task<IActionResult> Add(EditProductDTO productDTO)
		{
			var result = await _productService.AddProductAsync(productDTO);
			return Ok(result);
		}
		[HttpPut("{id}update")]
		public async Task<IActionResult> Update(Guid id,EditProductDTO productDTO)
		{
			var result = await _productService.UpdateProductAsync(id,productDTO);
			return Ok(result);
		}
		[HttpPost("delete")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _productService.DeleteProductAsync(id);
			return Ok(result);
		}
		[HttpGet("{id:guid}/details")]
		public async Task<IActionResult> GetDetails(Guid id)
		{
			var result = await _productService.DetailsOfProductAsync(id);
			return Ok(result);
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAll(int page, int size, CancellationToken ct)
		{
			var result = await _productService.GetProductsAsync(page, size, ct);
			return Ok(result);
		}
		[HttpGet("byFilter")]
		public async Task<IActionResult> GetByFilter(int page, int size, [FromQuery] ProductSearchFilter filtr, CancellationToken ct)
		{
			var result = await _productService.FindProductsByFilterAsync(page, size, filtr, ct);
			return Ok(result);
		}
	}
}