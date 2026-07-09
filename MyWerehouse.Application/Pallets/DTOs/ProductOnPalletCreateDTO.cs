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
	public class ProductOnPalletCreateDTO : IMapFrom<ProductOnPallet>
	{
		public Guid ProductId { get; init; }		
		public Guid PalletId { get; init; }
		public int Quantity { get; init; }
		public DateTime DateAdded { get; init; }
		public DateOnly? BestBefore { get; init; } 
	}
}