using MyWerehouse.Domain.Common;

namespace MyWerehouse.Domain.Issuing.IssueExceptions
{
	public class ProductOnPalletsAreNotTheSameAmountDomainException : DomainException
	{
		public Guid ProductId { get; }
		public int OldQuantity { get; }
		public int NewQuantity { get; }

		public ProductOnPalletsAreNotTheSameAmountDomainException(
			Guid productId,
			int oldQuantity,
			int newQuantity)
			: base($"Product {productId} quantities on replacement pallets do not match: {oldQuantity} and {newQuantity}.")
		{
			ProductId = productId;
			OldQuantity = oldQuantity;
			NewQuantity = newQuantity;
		}
	}
}
