using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class CannotReplaceTheSamePalletDomainException : DomainException
	{
		public Guid PalletId { get; }

		public CannotReplaceTheSamePalletDomainException(Guid palletId)
			: base($"Pallet {palletId} cannot be replaced with itself.")
		{
			PalletId = palletId;
		}
	}
}
