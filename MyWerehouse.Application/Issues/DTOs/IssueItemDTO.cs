using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public record IssueItemDTO : IMapFrom<IssueItem> 
	{	
		public Guid ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductSKU { get; set; }
		public int Quantity { get; set; }
		public DateOnly BestBefore { get; set; }	
		public void Mapping(Profile profile)
		{
			profile.CreateMap<IssueItem, IssueItemDTO>()
				.ForMember(dest=>dest.ProductName, opt=>opt.MapFrom(static src=> src.Product.Name))
				.ForMember(dest=>dest.ProductSKU, opt=>opt.MapFrom(static src=> src.Product.SKU));
		}
	}
}
