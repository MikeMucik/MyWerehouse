using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.CategoryModels;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	
	[ApiController]
	[Route("api/categories")]
	public class CategoriesController(ICategoryService categoryService) : ControllerBase
	{
		private readonly ICategoryService _categoryService = categoryService;

		//Dodaj kategorię
		[HttpPost]
		public async Task<IActionResult> Create(CategoryDTO categoryDto)		
			=> (await _categoryService.AddCategoryAsync(categoryDto))
				.ToActionResult();

		[HttpGet("{id:int}")]
		public async Task<IActionResult> Get(int id)
			=> (await _categoryService.GetCategoryByIdAsync(id))
			.ToActionResult();

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, CategoryDTO categoryDto)
			=> (await _categoryService.UpdateCategoryAsync(id, categoryDto))
			.ToActionResult();

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
			=> (await _categoryService.DeleteCategoryAsync(id))
			.ToActionResult();

		[HttpGet]
		public async Task<IActionResult> GetAll(
			[FromQuery] int page = 1,
			[FromQuery] int size = 10,
			CancellationToken ct = default)
			=> (await _categoryService.GetCategoriesAsync(page, size, ct))
			.ToActionResult();		
	}
}
