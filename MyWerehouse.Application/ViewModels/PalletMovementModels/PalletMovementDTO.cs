using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MyWerehouse.Application.Mapping;
using MyWerehouse.Application.ViewModels.PalletMovementDetailModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.ViewModels.PalletMovementModels
{
	public class PalletMovementDTO :IMapFrom<PalletMovement>
	{
		public int Id { get; set; }
		public string PalletId { get; set; }		
		public int? SourceLocationId { get; set; }		
		public int? DestinationLocationId { get; set; }		
		public ReasonMovement Reason { get; set; } // np. "Picking", "Correction", "Merge"
		public string? PerformedBy { get; set; } // opcjonalnie: user		
		public ICollection<PalletMovementDetailDTO> PalletMovementDetailsDTO { get; set; } = new List<PalletMovementDetailDTO>();
		public DateTime MovementDate { get; set; }
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PalletMovement,PalletMovementDTO>()				
				.ForMember(dest=>dest.PalletMovementDetailsDTO, opt=>opt.MapFrom(src=>src.PalletMovementDetails));
		}
	}
}
