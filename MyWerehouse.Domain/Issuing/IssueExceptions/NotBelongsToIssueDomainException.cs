using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class NotBelongsToIssueDomainException : DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }
		public Guid IssueId { get; }
		public int IssueNumber { get; }

		public NotBelongsToIssueDomainException(
			Guid palletId,
			string palletNumber,
			Guid issueId,
			int issueNumber)
			: base($"Pallet {palletNumber} ({palletId}) does not belong to issue {issueNumber} ({issueId}).")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
			IssueId = issueId;
			IssueNumber = issueNumber;
		}
	}
}
