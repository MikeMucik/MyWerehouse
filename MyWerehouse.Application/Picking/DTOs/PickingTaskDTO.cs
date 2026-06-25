using AutoMapper;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Picking.DTOs
{
	public class PickingTaskDTO : IMapFrom<PickingTask>
	{
		public Guid Id { get; init; }
		public Guid IssueId { get; init; }
		public int IssueNumber { get; init; }
		public Guid? SourcePalletId { get; init; }      
		public string? SourcePalletNumber { get; init; }      
		public Guid ProductId { get; init; }
		public string SKU { get; init; }
		public int RequestedQuantity { get; init; }
		public int PickedQuantity { get; init; }//faktyczna pobrana ilość
		public PickingStatus PickingStatus { get; init; }
		public DateOnly? BestBefore { get; init; }
		public int RampNumber { get; init; } //lokalizacja pickingu
		public void Mapping(Profile profile)
		{
			profile.CreateMap<PickingTask, PickingTaskDTO>()
				.ForMember(dest => dest.IssueNumber, opt => opt.MapFrom(static src => src.Issue.IssueNumber))
				.ForMember(dest=>dest.SourcePalletId, opt=> opt.MapFrom(static src => src.VirtualPallet.PalletId))		
				.ForMember(dest=>dest.SourcePalletNumber, opt=> opt.MapFrom(static src => src.VirtualPallet.Pallet.PalletNumber))
				.ForMember(dest=>dest.SKU, opt=>opt.MapFrom(static src => src.Product.SKU));			
		}
	}
}
