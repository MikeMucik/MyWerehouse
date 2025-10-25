using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.PalletMovementDetailModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.HistoryDTO
{
	public class PalletMovementDTO :IMapFrom<PalletMovement>
	{
		public int Id { get; set; }
		public string PalletId { get; set; }		
		public int? SourceLocationId { get; set; }
		public string LocationSnapShotSource { get; set; }//
		public int? DestinationLocationId { get; set; }
		public string LocationSnapShotDestination { get; set; }//
		public ReasonMovement Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string PerformedBy { get; set; } // opcjonalnie: user		
		public ICollection<PalletMovementDetailDTO> PalletMovementDetailsDTO { get; set; } = new List<PalletMovementDetailDTO>();
		public DateTime MovementDate { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PalletMovement,PalletMovementDTO>()				
				.ForMember(dest=>dest.PalletMovementDetailsDTO, opt=>opt.MapFrom(src=>src.PalletMovementDetails))
				.ForMember(dest => dest.LocationSnapShotSource, opt => opt.MapFrom(src =>
				src.SourceLocation == null ?
				null
				: $"{src.SourceLocation.Bay}-{src.SourceLocation.Aisle}-{src.SourceLocation.Position}-{src.SourceLocation.Height}"))
				.ForMember(dest => dest.LocationSnapShotDestination, opt => opt.MapFrom(src =>
				src.DestinationLocation == null ?
				null
				: $"{src.SourceLocation.Bay} - {src.SourceLocation.Aisle} - {src.SourceLocation.Position} - {src.SourceLocation.Height}"))
			;
		}
	}
}
