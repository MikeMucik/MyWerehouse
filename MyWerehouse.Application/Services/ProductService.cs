using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
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
		public ProductService(
			IProductRepo repo,
			IMapper mapper,
			IReceiptRepo receiptRepo)
		{
			_productRepo = repo;
			_mapper = mapper;
			_receiptRepo = receiptRepo;
		}
		public ProductService(
			IProductRepo repo,
			IMapper mapper
			)
		{
			_productRepo = repo;
			_mapper = mapper;
			
		}

		public int AddProduct(AddProductDTO product)
		{
			if (string.IsNullOrWhiteSpace(product.Name) ||
				string.IsNullOrWhiteSpace(product.SKU) ||
				product.CategoryId == 0 ||
				product.Length == 0 ||
				product.Width == 0 ||
				product.Height == 0 ||
				product.Weight == 0)
			{
				throw new InvalidDataException("Uzupełnij wszystkie dane produktu.");
			}

			var productNew = _mapper.Map<Product>(product);
			var id = _productRepo.AddProduct(productNew);
			return id;
		}

		public void DeleteProduct(int productId)
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
		public AddProductDTO GetProductToEdit(int productId)
		{
			var product = _productRepo.GetProductById(productId);
			var productDTO = _mapper.Map<AddProductDTO>(product);
			return productDTO;
		}

		public void UpdateProduct(AddProductDTO product)
		{
			if (string.IsNullOrWhiteSpace(product.Name) ||
				string.IsNullOrWhiteSpace(product.SKU) ||
				product.CategoryId == 0 ||
				product.Length == 0 ||
				product.Width == 0 ||
				product.Height == 0 ||
				product.Weight == 0)
			{
				throw new InvalidDataException("Uzupełnij wszystkie dane produktu.");
			}

			var productNew = _mapper.Map<Product>(product);
			 _productRepo.UpdateProduct(productNew);
		}

		public DetailsOfProductDTO DetailsOfProduct(int productId)
		{
			var product = _productRepo.GetProductById(productId);
			var productDTO = _mapper.Map<DetailsOfProductDTO>(product);
			return productDTO;
		}

		
	}
}
