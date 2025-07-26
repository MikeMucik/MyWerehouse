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
		private readonly IValidator<AddProductDTO> _productValidator;
		private readonly IProductOnPalletRepo _productOnPalletRepo;

		public ProductService(
			IProductRepo repo,
			IMapper mapper,
			WerehouseDbContext werehouseDbContext,
			IReceiptRepo? receiptRepo = null,
			IValidator<AddProductDTO>? productValidator = null,
			IProductOnPalletRepo productOnPalletRepo = null)
		{
			_productRepo = repo;
			_mapper = mapper;
			_werehouseDbContext = werehouseDbContext;
			_receiptRepo = receiptRepo;
			_productValidator = productValidator;
			_productOnPalletRepo = productOnPalletRepo;
		}
		public ProductService(
			IProductRepo repo,
			IMapper mapper,
			IValidator<AddProductDTO>? productValidator = null)
		{
			_productRepo = repo;
			_mapper = mapper;
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
			//var id = 
				await _productRepo.AddProductAsync(productNew);
			await _werehouseDbContext.SaveChangesAsync();
			return productNew.Id;
		}		
		public async Task DeleteProductAsync(int productId)
		{
			if (await _productRepo.GetProductByIdAsync(productId) != null)
			{
				var filter = new IssueReceiptSearchFilter
				{
					ProductId = productId
				};
				var receipt = _receiptRepo.GetReceiptByFilter(filter);
				if (!(await receipt.AnyAsync()))
				{
					await _productRepo.DeleteProductByIdAsync(productId);
				}
				else
				{
					await _productRepo.SwitchOffProductAsync(productId);
				}
			}
			else { throw new InvalidDataException("Brak produktu o tym numerze"); }
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
