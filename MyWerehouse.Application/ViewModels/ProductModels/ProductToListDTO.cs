using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Products.Models;

namespace MyWerehouse.Application.ViewModels.ProductModels
{
	public class ProductToListDTO : IMapFrom<Product>
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public string SKU { get; set; }
		public string Category { get; set; }
		//public bool IsDeleted { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<Product,  ProductToListDTO>()
				.ForMember(dest=>dest.Category, opt=>opt.MapFrom(src=>src.Category.Name));
		}
	}
}
