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

namespace MyWerehouse.Application.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepo _productRepo;
		private readonly IMapper _mapper;
		private readonly IReceiptRepo _receiptRepo;
		private readonly IValidator<AddProductDTO> _productValidator;

		public ProductService(
			IProductRepo repo,
			IMapper mapper,
			IReceiptRepo? receiptRepo = null,
			IValidator<AddProductDTO>? productValidator = null)
		{
			_productRepo = repo;
			_mapper = mapper;
			_receiptRepo = receiptRepo;
			_productValidator = productValidator;
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

		public int AddProduct(AddProductDTO product)
		{
			var validationResult = _productValidator.Validate(product);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = _productRepo.FindProducts(new ProductSearchFilter { ProductName = product.Name });
			if (existingProduct.Any())
			{
				throw new InvalidDataException("Produkt o tej nazwie już istnieje.");
			}
			var productNew = _mapper.Map<Product>(product);
			var id = _productRepo.AddProduct(productNew);
			return id;
		}
		public async Task<int> AddProductAsync(AddProductDTO product)
		{
			var validationResult = _productValidator.Validate(product);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var existingProduct = _productRepo.FindProducts(new ProductSearchFilter { ProductName = product.Name });
			if (await existingProduct.AnyAsync())
			{
				throw new InvalidDataException("Produkt o tej nazwie już istnieje.");
			}
			var productNew = _mapper.Map<Product>(product);
			var id = await _productRepo.AddProductAsync(productNew);
			return id;
		}

		public void DeleteProduct(int productId)
		{
			if (_productRepo.GetProductById(productId) != null)
			{
				var filter = new IssueReceiptSearchFilter
				{
					ProductId = productId
				};
				var receipt = _receiptRepo.GetReceiptByFilter(filter);
				if (!receipt.Any())
				{
					_productRepo.DeleteProductById(productId);
				}
				else
				{
					_productRepo.SwitchOffProduct(productId);
				}
			}
			else { throw new InvalidDataException("Brak produktu o tym numerze"); }
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
		}
		public AddProductDTO GetProductToEdit(int productId)
		{
			var product = _productRepo.GetProductById(productId);
			var productDTO = _mapper.Map<AddProductDTO>(product);
			return productDTO;
		}
		public async Task<AddProductDTO> GetProductToEditAsync(int productId)
		{
			var product = await _productRepo.GetProductByIdAsync(productId);
			var productDTO = _mapper.Map<AddProductDTO>(product);
			return productDTO;
		}
		public void UpdateProduct(AddProductDTO product)
		{
			var validationResult = _productValidator.Validate(product);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			//if (string.IsNullOrWhiteSpace(product.Name) ||
			//	string.IsNullOrWhiteSpace(product.SKU) ||
			//	product.CategoryId == 0 ||
			//	product.Length == 0 ||
			//	product.Width == 0 ||
			//	product.Height == 0 ||
			//	product.Weight == 0)
			//{
			//	throw new InvalidDataException("Uzupełnij wszystkie dane produktu.");
			//}
			var productNew = _mapper.Map<Product>(product);
			_productRepo.UpdateProduct(productNew);
		}
		public async Task UpdateProductAsync(AddProductDTO product)
		{
			var validationResult = _productValidator.Validate(product);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var productNew = _mapper.Map<Product>(product);
			await _productRepo.UpdateProductAsync(productNew);
		}
		public DetailsOfProductDTO DetailsOfProduct(int productId)
		{
			var product = _productRepo.GetProductById(productId);
			var productDTO = _mapper.Map<DetailsOfProductDTO>(product);
			return productDTO;
		}
		public async Task<DetailsOfProductDTO> DetailsOfProductAsync(int productId)
		{
			var product = await _productRepo.GetProductByIdAsync(productId);
			var productDTO = _mapper.Map<DetailsOfProductDTO>(product);
			return productDTO;
		}
		public ListProductsDTO GetProducts(int pageSize, int PageNumber)
		{
			var products = _productRepo.GetAllProducts()
				.OrderBy(p => p.Id)
				.ProjectTo<ProductToListDTO>(_mapper.ConfigurationProvider)
				//.ToList
				;
			var productToShow = products
				.Skip(pageSize * (PageNumber - 1))
				.Take(pageSize)
				.ToList()
				;
			var productList = new ListProductsDTO()
			{
				Products = productToShow,
				PageSize = pageSize,
				CurrentPage = PageNumber,
				Count = products.Count()
			};
			return productList;
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
		public ListProductsDTO FindProductsByFilter(int pageSize, int PageNumber, ProductSearchFilter filter)
		{
			var products = _productRepo.FindProducts(filter)
				.OrderBy(p => p.Id)
				.ProjectTo<ProductToListDTO>(_mapper.ConfigurationProvider)
				//.ToList
				;
			var productToShow = products
				.Skip(pageSize * (PageNumber - 1))
				.Take(pageSize)
				.ToList();
			var productList = new ListProductsDTO()
			{
				Products = productToShow,
				PageSize = pageSize,
				CurrentPage = PageNumber,
				Count = products.Count()
			};
			return productList;
		}
		public async Task<ListProductsDTO> FindProductsByFilterAsync(int pageSize, int PageNumber, ProductSearchFilter filter)
		{
			var products = _productRepo.FindProducts(filter)
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
	}
}
