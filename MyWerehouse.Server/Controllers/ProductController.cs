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
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(Guid id)
		{
			var result = await _productService.GetProductToEditAsync(id);
			return Ok(result);
		}
		[HttpPost("add")]
		public async Task<IActionResult> Add(AddProductDTO productDTO)
		{
			var result = await _productService.AddProductAsync(productDTO);
			return Ok(result);
		}
		[HttpPost("update")]
		public async Task<IActionResult> Update(AddProductDTO productDTO)
		{
			var result = await _productService.UpdateProductAsync(productDTO);
			return Ok(result);
		}
		[HttpPost("delete")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var result = await _productService.DeleteProductAsync(id);
			return Ok(result);
		}
		[HttpGet("{id}/details")]
		public async Task<IActionResult> GetDetails(Guid id)
		{
			var result = await _productService.DetailsOfProductAsync(id);
			return Ok(result);
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAll(int page, int size)
		{
			var result = await _productService.GetProductsAsync(page, size);
			return Ok(result);
		}
		[HttpGet("byFilter")]
		public async Task<IActionResult> GetByFilter(int page, int size, ProductSearchFilter filtr)
		{
			var result = await _productService.FindProductsByFilterAsync(page, size, filtr);
			return Ok(result);
		}
	}
}
