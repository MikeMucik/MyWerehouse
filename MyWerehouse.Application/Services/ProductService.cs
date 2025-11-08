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
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

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
		public async Task<int> AddProductAsync(AddProductDTO productDTO)
		{
			var validationResult = _productValidator.Validate(productDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = _productRepo.FindProducts(new ProductSearchFilter { ProductName = productDTO.Name });
			if (await existingProduct.AnyAsync())
			{
				throw new InvalidDataException("Produkt o tej nazwie już istnieje.");
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
			return product.Id;
		}
		public async Task DeleteProductAsync(int productId)
		{
			var product = await _productRepo.GetProductByIdAsync(productId) ?? throw new DomainException("Brak produktu o tym numerze");
			var filter = new IssueReceiptSearchFilter
			{
				ProductId = productId
			};
			var receipt = _receiptRepo.GetReceiptByFilter(filter);
			if (await receipt.AnyAsync())
			{
				_productRepo.SwitchOffProduct(product);
				//await _productRepo.SwitchOffProductAsync(productId);
			}
			else
			{
				_productRepo.DeleteProduct(product);
			}
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<AddProductDTO> GetProductToEditAsync(int productId)
		{
			var product = await _productRepo.GetProductByIdAsync(productId);
			var productDTO = _mapper.Map<AddProductDTO>(product);
			return productDTO;
		}
		public async Task UpdateProductAsync(AddProductDTO productDTO)
		{
			var validationResult = _productValidator.Validate(productDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = await _productRepo.GetProductToEditAsync(productDTO.Id);
			if (existingProduct == null)
			{
				throw new InvalidDataException($"Produkt o numerze {productDTO.Id} nie istnieje");
			}
			_mapper.Map(productDTO, existingProduct);
			await _werehouseDbContext.SaveChangesAsync();
		}
		public async Task<DetailsOfProductDTO> DetailsOfProductAsync(int productId)
		{
			var product = await _productRepo.GetProductToEditAsync(productId);
			var productDTO = _mapper.Map<DetailsOfProductDTO>(product);
			return productDTO;
		}
		public async Task<ListProductsDTO> GetProductsAsync(int pageSize, int PageNumber)
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
			return productList;
		}
		public async Task<ListProductsDTO> FindProductsByFilterAsync(int pageSize, int pageNumber, ProductSearchFilter filter)
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
			return productList;
		}
	}
}
