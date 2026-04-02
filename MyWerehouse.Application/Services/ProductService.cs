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
using MyWerehouse.Domain.DomainExceptions;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Invetories.Models;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Domain.Products.Models;
using MyWerehouse.Domain.Products.Filters;
using MyWerehouse.Application.Common.Results;
using MediatR;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly IValidator<AddProductDTO> _productValidator;

		public ProductService(
			IProductRepo repo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IInventoryRepo inventoryRepo,
			IReceiptRepo? receiptRepo = null,
			IValidator<AddProductDTO>? productValidator = null)
		{
			_productRepo = repo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_inventoryRepo = inventoryRepo;
			_receiptRepo = receiptRepo;
			_productValidator = productValidator;
		}

		public ProductService(
			IProductRepo repo,
			IMapper mapper)
		{
			_productRepo = repo;
			_mapper = mapper;
		}
		public async Task<AppResult<Guid>> AddProductAsync(AddProductDTO productDTO)
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
			var productNew = _mapper.Map<Product>(productDTO);
			var product = _productRepo.AddProduct(productNew);
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
				//_productRepo.SwitchOffProduct(product);
			}
			else
			{
				_productRepo.DeleteProduct(product);
			}
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<AddProductDTO>> GetProductToEditAsync(Guid id)
		{
			var product = await _productRepo.GetProductByIdAsync(id);
			var productDTO = _mapper.Map<AddProductDTO>(product);
			return AppResult<AddProductDTO>.Success( productDTO);
		}
		public async Task<AppResult<Unit>> UpdateProductAsync(AddProductDTO productDTO)
		{
			var validationResult = _productValidator.Validate(productDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = await _productRepo.GetProductToEditAsync(productDTO.Id);
			if (existingProduct == null)
			{
				return AppResult<Unit>.Fail($"Produkt o numerze {productDTO.Id} nie istnieje", ErrorType.NotFound);
				//throw new NotFoundProductException($"Produkt o numerze {productDTO.Id} nie istnieje");
			}
			_mapper.Map(productDTO, existingProduct);
			await _werehouseDbContext.SaveChangesAsync();
			return AppResult<Unit>.Success(Unit.Value);
		}
		public async Task<AppResult<DetailsOfProductDTO>> DetailsOfProductAsync(Guid id)
		{
			var product = await _productRepo.GetProductToEditAsync(id);
			if (product == null) return AppResult<DetailsOfProductDTO>.Fail("Brak elementów do wyświetlenia", ErrorType.NotFound);
			var productDTO = _mapper.Map<DetailsOfProductDTO>(product);
			
			return AppResult<DetailsOfProductDTO>.Success(productDTO);
		}
		public async Task<AppResult<ListProductsDTO>> GetProductsAsync(int pageSize, int PageNumber)
		{
			var products = _productRepo.GetAllProducts()
				.OrderBy(p => p.Id)
				.ProjectTo<ProductToListDTO>(_mapper.ConfigurationProvider);
			var productToShow = await products
				.Skip(pageSize * (PageNumber - 1))
				.Take(pageSize)
				.ToListAsync();
			var productList = new ListProductsDTO()
			{
				Products = productToShow,
				PageSize = pageSize,
				CurrentPage = PageNumber,
				Count = await products.CountAsync()
			};
			return AppResult<ListProductsDTO>.Success(productList);
		}
		public async Task<AppResult<ListProductsDTO>> FindProductsByFilterAsync(int pageSize, int pageNumber, ProductSearchFilter filter)
		{
			pageNumber = pageNumber <= 1 ? 1 : pageNumber;
			var products = _productRepo.FindProducts(filter)
				.OrderBy(p => p.Id)
				.ProjectTo<ProductToListDTO>(_mapper.ConfigurationProvider);

			var countProducts = products.CountAsync();
			var productToShow = products
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync();

			await Task.WhenAll(countProducts, productToShow);

			var productList = new ListProductsDTO()
			{
				Products = productToShow.Result,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count = countProducts.Result,
			};
			return AppResult<ListProductsDTO>.Success( productList);
		}
	}
}
