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
		//tu trzeba się zastanowić czy to w ogóle potrzebne poniższe dwie pozycje 
		public string? SourcePalletId { get; init; }//paleta źródłowa na nią wraca towar lub do której dodajemy
		public string? DestinationPalletId { get; init; }//paleta nowa jeśli nie ma do czego dołaczyć ???
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
