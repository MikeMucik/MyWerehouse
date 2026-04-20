using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;

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
			return Ok(result);
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _categoryService.GetCategoryByIdAsync(id);
			return Ok(result);
		}
		[HttpPost("update")]
		public async Task<IActionResult> Update(CategoryDTO category)
		{
			var result = await _categoryService.UpdateCategoryAsync(category);
			return Ok(result);
		}
		[HttpPost("delete")]//bo może tylko tylko ukrycie przez historię
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _categoryService.DeleteCategoryAsync(id);
			return Ok(result);
		}
		[HttpGet("all")]
		public async Task<IActionResult> GetAll(int page, int size)//można zamienić na [FromQuery] +DTO
		{
			var result = await _categoryService.GetCategoriesAsync(page,size);
			return Ok(result);
		}
	}
}
