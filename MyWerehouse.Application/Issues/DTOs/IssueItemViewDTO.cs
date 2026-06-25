using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Issues.DTOs
{
	public class IssueItemViewDTO : IMapFrom<IssueItem>
	{
		public Guid ProductId { get; init; }
		public string ProductName { get; init; }
		public string ProductSKU { get; init; }
		public int Quantity { get; init; }
		public DateOnly BestBefore { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<IssueItem, IssueItemViewDTO>()
				.ForMember(dest => dest.ProductName, opt => opt.MapFrom(static src => src.Product.Name))
				.ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(static src => src.Product.SKU));
		}
	}
}
