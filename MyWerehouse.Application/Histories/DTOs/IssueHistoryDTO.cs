using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Issuing.Models;

namespace MyWerehouse.Application.Histories.DTOs
{
	public class IssueHistoryDTO
	{
		public int Id { get; set; }
		public DateTime IssueDateTimeCreate { get; set; }
		public DateTime IssueDateTimeSend { get; set; }
		public int CLientId { get; set; }
		public IssueStatus IssueStatus { get; set; }
		public string PerformedBy { get; set; } = string.Empty;
		public ICollection<PalletListDTO> ListDTOs { get; set; } = new List<PalletListDTO>();
	}
}
