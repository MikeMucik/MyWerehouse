using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.DTOs
{
	public class ReversePickingDTO : IMapFrom<ReversePicking>
	{
		public Guid Id { get; init; }
		public required string PickingPalletId { get; init; }
		public string? SourcePalletId { get; init; }
		public string? DestinationPalletId { get; init; }
		public Guid ProductId { get; init; }
		public DateOnly? BestBefore { get; init; }
		public int Quantity { get; init; }
		public ReversePickingStatus Status { get; init; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<ReversePicking,  ReversePickingDTO>();
		}
	}
}
