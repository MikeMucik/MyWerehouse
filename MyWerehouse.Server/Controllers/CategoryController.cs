using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	
	[ApiController]
	[Route("api/category")]
	public class CategoryController : ControllerBase
	{
		private readonly ICategoryService _categoryService;
		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		//Dodaj kategorię
		[HttpPost("add")]
		public async Task<IActionResult> Add(CategoryDTO category)
		{
			var result = await _categoryService.AddCategoryAsync(category);
			return result.ToActionResult();
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _categoryService.GetCategoryByIdAsync(id);
			return result.ToActionResult();
		}
		[HttpPut("{id}update")]
		public async Task<IActionResult> Update(int id, CategoryDTO category)
		{
			var result = await _categoryService.UpdateCategoryAsync(id, category);
			return result.ToActionResult();
		}
		[HttpPost("delete")]//bo może tylko tylko ukrycie przez historię
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _categoryService.DeleteCategoryAsync(id);
			return result.ToActionResult();
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAll(int page, int size, CancellationToken ct)//można zamienić na [FromQuery] +DTO
		{
			var result = await _categoryService.GetCategoriesAsync(page,size, ct);
			return result.ToActionResult();
		}
	}
}
