using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ReversePickings.DTOs
{
	public class ReversePickingDTO : IMapFrom<ReversePicking>
	{
		public int Id { get; set; }
		public required string PickingPalletId { get; set; }
		//tu trzeba się zastanowić czy to w ogóle potrzebne poniższe dwie pozycje 
		public string? SourcePalletId { get; set; }//paleta źródłowa na nią wraca towar lub do której dodajemy
		public string? DestinationPalletId { get; set; }//paleta nowa jeśli nie ma do czego dołaczyć
		public int ProductId { get; set; }
		public DateOnly? BestBefore { get; set; }
		public int Quantity { get; set; }
		public ReversePickingStatus Status { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<ReversePicking,  ReversePickingDTO>();
		}
	}
}
