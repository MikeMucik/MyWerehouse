using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class NotOneProductsOnPalletDomainException : DomainException
	{
		public Guid PalletId { get; }
		public string PalletNumber { get; }

		public NotOneProductsOnPalletDomainException(Guid palletId, string palletNumber)
			: base($"Pallet {palletNumber} ({palletId}) must contain exactly one product to be used as a replacement.")
		{
			PalletId = palletId;
			PalletNumber = palletNumber;
		}
	}
}
