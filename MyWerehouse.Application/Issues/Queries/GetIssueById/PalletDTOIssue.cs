using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Queries.GetIssueById
{
	public class PalletDTOIssue 
	{
		public Guid Id { get; init; }
		public string PalletNumber { get; init; } = string.Empty;
		public int LocationId { get; init; }
		public string LocationSnapShot { get; init; } = string.Empty;
		public PalletStatus Status { get; init; }
		public ICollection<ProductOnPalletDTO> ProductsOnPallet { get; init; } = new HashSet<ProductOnPalletDTO>();		
	}
}
