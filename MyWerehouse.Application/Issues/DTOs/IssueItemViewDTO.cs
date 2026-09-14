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
	public class IssueItemViewDTO
	{
		public Guid ProductId { get; init; }
		public string ProductName { get; init; } = string.Empty;
		public string ProductSKU { get; init; } = string.Empty;
		public int Quantity { get; init; }
		public DateOnly? BestBefore { get; init; }		
	}
}
