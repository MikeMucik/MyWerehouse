using FluentValidation;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class EditProductDTO 
	{
		public Guid Id { get; init; }
		public required string Name { get; init; }
		public required string SKU { get; init; }
		public int CategoryId { get; init; }
		public bool? IsDeleted { get; init; } = false;
		public int CartonsPerPallet { get; init; }
		public int Length { get; init; } //cm
		public int Height { get; init; } //cm
		public int Width { get; init; } //cm
		public int Weight { get; init; } //kg
		public required string Description { get; init; }
				
	}
	public class EditProductDTOValidation : AbstractValidator<EditProductDTO>
	{
		public EditProductDTOValidation()
		{
			RuleFor(p => p.Name).NotEmpty().WithMessage("Product name is required.");
			RuleFor(p => p.SKU).NotEmpty().WithMessage("Product SKU is required.");
			RuleFor(p => p.CartonsPerPallet).GreaterThan(0).WithMessage("Cartons per pallet must be greater than zero.");
			RuleFor(p => p.CategoryId).NotNull().WithMessage("Product category is required.");
			RuleFor(p => p.CategoryId).GreaterThan(0).WithMessage("Product category is required.");
			RuleFor(p => p.Height).GreaterThan(0).WithMessage("Product height must be greater than zero.");
			RuleFor(p => p.Width).GreaterThan(0).WithMessage("Product width must be greater than zero.");
			RuleFor(p => p.Weight).GreaterThan(0).WithMessage("Product weight must be greater than zero.");
			RuleFor(p => p.Length).GreaterThan(0).WithMessage("Product length must be greater than zero.");
		}
	}
}
