using FluentValidation;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.CategoryModels
{
	public class CategoryDTO 
	{		
		public required string Name { get; init; }			
	}
	public class CategoryDTOValidation : AbstractValidator<CategoryDTO>
	{
		public CategoryDTOValidation()
		{			
			RuleFor(g => g.Name).NotNull().WithMessage("Category name is required.");
			RuleFor(g => g.Name).Must(value=> !string.IsNullOrEmpty(value)).WithMessage("Category name is required.");
		}
	}
}
