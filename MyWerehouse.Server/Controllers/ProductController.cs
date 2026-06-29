using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Server.Extensions;

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
			return result.ToActionResult();
		}
		[HttpPost("add")]
		public async Task<IActionResult> Add(EditProductDTO productDTO)
		{
			var result = await _productService.AddProductAsync(productDTO);
			return result.ToActionResult();
		}
		[HttpPut("{id}update")]
		public async Task<IActionResult> Update(Guid id,EditProductDTO productDTO)
		{
			var result = await _productService.UpdateProductAsync(id,productDTO);
			return result.ToActionResult();
		}
		[HttpPost("delete")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _productService.DeleteProductAsync(id);
			return result.ToActionResult();
		}
		[HttpGet("{id:guid}/details")]
		public async Task<IActionResult> GetDetails(Guid id)
		{
			var result = await _productService.DetailsOfProductAsync(id);
			return result.ToActionResult();
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAll(int page, int size, CancellationToken ct)
		{
			var result = await _productService.GetProductsAsync(page, size, ct);
			return result.ToActionResult();
		}
		[HttpGet("byFilter")]
		public async Task<IActionResult> GetByFilter(int page, int size, [FromQuery] ProductSearchFilter filtr, CancellationToken ct)
		{
			var result = await _productService.FindProductsByFilterAsync(page, size, filtr, ct);
			return result.ToActionResult();
		}
	}
}