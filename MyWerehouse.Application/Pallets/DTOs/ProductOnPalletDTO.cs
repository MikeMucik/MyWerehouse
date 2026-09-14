using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Pallets.DTOs
{
	public class ProductOnPalletDTO
	{
		public Guid ProductId { get; init; }
		public string ProductSKU { get; init; } = string.Empty;
		public string ProductName { get; init; } = string.Empty;
		public int Quantity { get; init; }
		public DateTime DateAdded { get; init; }
		public DateOnly? BestBefore { get; init; } // Może być null, jeśli produkt nie ma daty ważności
	}
}
