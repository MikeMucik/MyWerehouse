using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class ProductOnPalletsAreNotTheSameBBDomainException : DomainException
	{
		public Guid ProductId { get; }
		public DateOnly? OldBestBefore { get; }
		public DateOnly? NewBestBefore { get; }

		public ProductOnPalletsAreNotTheSameBBDomainException(
			Guid productId,
			DateOnly? oldBestBefore,
			DateOnly? newBestBefore)
			: base($"Product {productId} best-before dates on replacement pallets do not match: {oldBestBefore} and {newBestBefore}.")
		{
			ProductId = productId;
			OldBestBefore = oldBestBefore;
			NewBestBefore = newBestBefore;
		}
	}
}
