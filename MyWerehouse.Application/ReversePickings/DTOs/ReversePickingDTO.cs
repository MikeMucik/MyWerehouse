using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.ReversePickings.Models;

namespace MyWerehouse.Application.ReversePickings.DTOs
{
	public class ReversePickingDTO
	{
		public Guid Id { get; init; }
		public required string PickingPalletNumber { get; init; }
		public string? SourcePalletNumber { get; init; }
		public required string ProductSKU { get; init; }		
		public DateOnly? BestBefore { get; init; }
		public int Quantity { get; init; }
		public ReversePickingStatus Status { get; init; }		
	}
}
