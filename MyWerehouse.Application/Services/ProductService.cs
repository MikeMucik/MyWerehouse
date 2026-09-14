using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Application.Common.Results;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Domain.Inventories.Models;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Interfaces;

namespace MyWerehouse.Application.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepo _productRepo;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IProductReadService _productReadService;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly ICategoryRepo _categoryRepo;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IValidator<CreateProductDTO> _createProductValidator;
		private readonly IValidator<EditProductDTO> _productValidator;
		private readonly IDateTimeProvider _dateTimeProvider;

		public ProductService(
			IProductRepo repo,
			IUnitOfWork unitOfWork,
			IProductReadService productReadService,
			IInventoryRepo inventoryRepo,
			ICategoryRepo categoryRepo,
			IReceiptRepo receiptRepo,
			IValidator<CreateProductDTO> createProductValidator,
			IValidator<EditProductDTO> productValidator,
			IDateTimeProvider dateTimeProvider)
		{
			_productRepo = repo;
			_unitOfWork = unitOfWork;
			_productReadService = productReadService;
			_inventoryRepo = inventoryRepo;
			_categoryRepo = categoryRepo;
			_receiptRepo = receiptRepo;
			_createProductValidator = createProductValidator;
			_productValidator = productValidator;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<AppResult<Guid>> AddProductAsync(CreateProductDTO productDTO, CancellationToken ct)
		{
			var validationResult = await _createProductValidator.ValidateAsync(productDTO, ct);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			if (await _productRepo.AlreadyExist(productDTO.Name, productDTO.SKU))
			{
				return AppResult<Guid>.Fail("A product with this name/SKU already exists.");
			}
			var existCategory = await _categoryRepo.GetCategoryByIdAsync(productDTO.CategoryId, ct);
			if (existCategory == null)
			{
				return AppResult<Guid>.Fail($"Category {productDTO.CategoryId} does not exist.");
			}
			if (existCategory.IsDeleted)
			{
				return AppResult<Guid>.Fail($"Category {productDTO.CategoryId} is inactive.", ErrorType.Conflict);
			}
			var productPrepare = Product.Create(
				productDTO.Name,
				productDTO.SKU,
				_dateTimeProvider.UtcNow,
				productDTO.CategoryId,
				productDTO.CartonsPerPallet,
				productDTO.Length,
				productDTO.Height,
				productDTO.Width,
				productDTO.Weight,
				productDTO.Description);
			var product = _productRepo.AddProduct(productPrepare);
			var inventory = Inventory.CreateStockItem(product.Id, 0, _dateTimeProvider.UtcNow);
			_inventoryRepo.AddInventory(inventory);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Guid>.Success(product.Id);
		}
		public async Task<AppResult<Unit>> DeleteProductAsync(Guid id, CancellationToken ct)
		{
			var product = await _productRepo.GetProductByIdAsync(id, ct);
			if (product == null)
			{
				return AppResult<Unit>.Fail("No product with this ID was found.");
			}
			if (await _receiptRepo.HasReceiptProduct(id, ct))
			{
				product.Hide();
			}
			else
			{
				_productRepo.DeleteProduct(product);
			}
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<EditProductDTO>> GetProductToEditAsync(Guid id, CancellationToken ct)
		{
			var product = await _productReadService.GetProductToEditAsync(id, ct);
			if (product == null)
			{
				return AppResult<EditProductDTO>.Fail($"Product {id} does not exist.");
			}
			return AppResult<EditProductDTO>.Success(product);
		}
		public async Task<AppResult<Unit>> UpdateProductAsync(Guid id, EditProductDTO productDTO, CancellationToken ct)
		{
			var validationResult = await _productValidator.ValidateAsync(productDTO, ct);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = await _productRepo.GetProductToEditAsync(id, ct);
			if (existingProduct == null)
			{
				return AppResult<Unit>.Fail($"Product {id} does not exist.");
			}
			var existCategory = await _categoryRepo.GetCategoryByIdAsync(productDTO.CategoryId, ct);
			if (existCategory == null)
			{
				return AppResult<Unit>.Fail($"Category {productDTO.CategoryId} does not exist.");
			}
			if (existCategory.IsDeleted)
			{
				return AppResult<Unit>.Fail($"Category {productDTO.CategoryId} is inactive.", ErrorType.Conflict);
			}
			existingProduct.ApplyChangesForProduct(
				productDTO.Name,
				productDTO.SKU,
				productDTO.CategoryId,
				productDTO.CartonsPerPallet,
				productDTO.Length,
				productDTO.Height,
				productDTO.Width,
				productDTO.Weight,
				productDTO.Description);
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<DetailsOfProductDTO>> DetailsOfProductAsync(Guid id, CancellationToken ct)
		{
			var product = await _productReadService.DetailsOfProductAsync(id, ct);
			if (product == null) return AppResult<DetailsOfProductDTO>.Fail("No product data to display.");
			return AppResult<DetailsOfProductDTO>.Success(product);
		}
		public async Task<AppResult<PagedResult<ProductDTO>>> GetProductsAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var products = await _productReadService.GetProductsAsync(pageNumber, pageSize, ct);
			return AppResult<PagedResult<ProductDTO>>.Success(products);
		}
		public async Task<AppResult<PagedResult<ProductDTO>>> FindProductsByFilterAsync(int pageNumber, int pageSize, ProductSearchFilter filter, CancellationToken ct)
		{
			var products = await _productReadService.FindProductsByFilterAsync(pageNumber, pageSize, filter, ct);
			return AppResult<PagedResult<ProductDTO>>.Success(products);
		}
	}
}
