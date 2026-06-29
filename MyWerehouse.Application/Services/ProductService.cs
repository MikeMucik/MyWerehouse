using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.ProductModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Application.Common.Results;
using MediatR;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Application.Common.Pagination;

namespace MyWerehouse.Application.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly ICategoryRepo _categoryRepo;
		private readonly IReceiptRepo _receiptRepo;		
		private readonly IValidator<EditProductDTO> _productValidator;

		public ProductService(
			IProductRepo repo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IInventoryRepo inventoryRepo,
			ICategoryRepo categoryRepo,
			IReceiptRepo? receiptRepo = null,
			IValidator<EditProductDTO>? productValidator = null)
		{
			_productRepo = repo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_inventoryRepo = inventoryRepo;
			_categoryRepo = categoryRepo;
			_receiptRepo = receiptRepo;
			_productValidator = productValidator;
		}

		public async Task<AppResult<Guid>> AddProductAsync(EditProductDTO productDTO)
		{
			var validationResult = _productValidator.Validate(productDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = _productRepo.FindProducts(new ProductSearchFilter { ProductName = productDTO.Name });
			if (await existingProduct.AnyAsync())
			{
				return AppResult<Guid>.Fail("Produkt o tej nazwie już istnieje.", ErrorType.NotFound);
			}
			var existCategory = await _categoryRepo.GetCategoryByIdAsync(productDTO.CategoryId);
			if (existCategory == null)
			{
				return AppResult<Guid>.Fail($"Kateogria o numerze {productDTO.CategoryId} nie istnieje.", ErrorType.NotFound);
			}
			if (existCategory.IsDeleted)
			{
				return AppResult<Guid>.Fail($"Kateogria o numerze {productDTO.CategoryId} jest nieaktywna.", ErrorType.Conflict);
			}
			var productPrepare = Product.Create(
				productDTO.Name,
				productDTO.SKU,
				productDTO.CategoryId,
				productDTO.CartonsPerPallet);
			productPrepare.AddDetails(
				productDTO.Length,
				productDTO.Height,
				productDTO.Width,
				productDTO.Weight,
				productDTO.Description);
			var product = _productRepo.AddProduct(productPrepare);
			var inventory = new Inventory
			{
				Product = product,
				Quantity = 0,
				LastUpdated = DateTime.UtcNow,
			};
			_inventoryRepo.AddInventory(inventory);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Guid>.Success(product.Id);
		}
		public async Task<AppResult<Unit>> DeleteProductAsync(Guid id)
		{
			var product = await _productRepo.GetProductByIdAsync(id);
			if (product == null)
			{
				return AppResult<Unit>.Fail("Brak produktu o tym numerze", ErrorType.NotFound);
			}
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = id
			};
			var receipt = _receiptRepo.GetReceiptByFilter(filter);
			if (await receipt.AnyAsync())
			{
				product.Hide();
			}
			else
			{
				_productRepo.DeleteProduct(product);
			}
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<EditProductDTO>> GetProductToEditAsync(Guid id)
		{
			var product = await _productRepo.GetProductToEditAsync(id);
			var productDTO = _mapper.Map<EditProductDTO>(product);
			return AppResult<EditProductDTO>.Success(productDTO);
		}
		public async Task<AppResult<Unit>> UpdateProductAsync(Guid id, EditProductDTO productDTO)
		{
			var validationResult = _productValidator.Validate(productDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = await _productRepo.GetProductToEditAsync(id);
			if (existingProduct == null)
			{
				return AppResult<Unit>.Fail($"Produkt o numerze {id} nie istnieje", ErrorType.NotFound);
			}
			var existCategory = await _categoryRepo.GetCategoryByIdAsync(productDTO.CategoryId);
			if (existCategory == null)
			{
				return AppResult<Unit>.Fail($"Kateogria o numerze {productDTO.CategoryId} nie istnieje.", ErrorType.NotFound);
			}
			if (existCategory.IsDeleted)
			{
				return AppResult<Unit>.Fail($"Kateogria o numerze {productDTO.CategoryId} jest nieaktywna.", ErrorType.Conflict);
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
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<DetailsOfProductDTO>> DetailsOfProductAsync(Guid id)
		{
			var product = await _productRepo.GetProductDetailsAsync(id);
			if (product == null) return AppResult<DetailsOfProductDTO>.Fail("Brak elementów do wyświetlenia", ErrorType.NotFound);
			var productDTO = _mapper.Map<DetailsOfProductDTO>(product);

			return AppResult<DetailsOfProductDTO>.Success(productDTO);
		}
		public async Task<AppResult<PagedResult<ProductDTO>>> GetProductsAsync(int pageNumber, int pageSize, CancellationToken ct)
		{
			var products = _productRepo.GetAllProducts();
			var productsOrdered = products
			.OrderBy(p => p.Id);
			var result = await productsOrdered
				.ProjectTo<ProductDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(pageNumber, pageSize, ct);
			return AppResult<PagedResult<ProductDTO>>.Success(result);
		}
		public async Task<AppResult<PagedResult<ProductDTO>>> FindProductsByFilterAsync(int pageNumber, int pageSize, ProductSearchFilter filter, CancellationToken ct)
		{
			var products = _productRepo.FindProducts(filter);
			var productsOrdered = products
				.OrderBy(p => p.Id);
			var result = await productsOrdered
				.ProjectTo<ProductDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(pageNumber, pageSize, ct);
			return AppResult<PagedResult<ProductDTO>>.Success(result);
		}
	}
}