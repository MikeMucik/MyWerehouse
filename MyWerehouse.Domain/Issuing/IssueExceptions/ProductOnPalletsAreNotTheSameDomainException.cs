using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class ProductOnPalletsAreNotTheSameDomainException : DomainException
	{
		public Guid OldProductId { get; }
		public Guid NewProductId { get; }

		public ProductOnPalletsAreNotTheSameDomainException(Guid oldProductId, Guid newProductId)
			: base($"Products on replacement pallets do not match: {oldProductId} and {newProductId}.")
		{
			OldProductId = oldProductId;
			NewProductId = newProductId;
		}
	}
}
